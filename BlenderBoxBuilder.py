bl_info = {
    "name": "Interactive Box Builder",
    "author": "SA_Toolbelt",
    "version": (1, 0, 0),
    "blender": (3, 0, 0),
    "location": "View3D > Sidebar > Box Builder",
    "description": "Interactively build custom box/rectangular tubes by extruding and tilting square rings with WASD/JIKL keys",
    "category": "Mesh",
}

import bpy
import bmesh
import math
from mathutils import Vector, Matrix
from bpy.props import FloatProperty, IntProperty, BoolProperty


# Class-level storage for resume vert indices (set by resume operator, read by modal)
_box_resume_vert_indices = None


class BOXBUILDER_OT_start(bpy.types.Operator):
    """Start interactive box building mode"""
    bl_idname = "mesh.box_builder_start"
    bl_label = "Start Box Builder"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        props = context.scene.box_builder

        # Create the starting square mesh
        mesh = bpy.data.meshes.new("BoxBuilder")
        obj = bpy.data.objects.new("BoxBuilder", mesh)
        context.collection.objects.link(obj)

        # Deselect all, then select and activate the new object
        bpy.ops.object.select_all(action='DESELECT')
        obj.select_set(True)
        context.view_layer.objects.active = obj

        # Initialize size transition to match starting dimensions
        props.start_width = props.width
        props.end_width = props.width
        props.width_steps_remaining = 0
        props.start_depth = props.depth
        props.end_depth = props.depth
        props.depth_steps_remaining = 0

        # Build initial square with bmesh
        bm = bmesh.new()
        w = props.width / 2.0
        d = props.depth / 2.0

        # 4 corners of the square (on XY plane at Z=0)
        v0 = bm.verts.new((-w, -d, 0.0))
        v1 = bm.verts.new(( w, -d, 0.0))
        v2 = bm.verts.new(( w,  d, 0.0))
        v3 = bm.verts.new((-w,  d, 0.0))
        verts = [v0, v1, v2, v3]

        # Create edges forming the square
        bm.edges.new((v0, v1))
        bm.edges.new((v1, v2))
        bm.edges.new((v2, v3))
        bm.edges.new((v3, v0))

        # Create the face
        bm.faces.new(verts)

        bm.to_mesh(mesh)
        bm.free()
        mesh.update()

        # Switch to edit mode for the modal operator
        bpy.ops.object.mode_set(mode='EDIT')

        # Launch the modal operator
        bpy.ops.mesh.box_builder_modal('INVOKE_DEFAULT')
        return {'FINISHED'}


class BOXBUILDER_OT_modal(bpy.types.Operator):
    """Modal operator that captures keypresses for box building"""
    bl_idname = "mesh.box_builder_modal"
    bl_label = "Box Builder Modal"
    bl_options = {'REGISTER', 'UNDO'}

    # Always 4 verts per ring for a square
    VERTS_PER_RING = 4

    def modal(self, context, event):
        props = context.scene.box_builder
        step = props.step_size
        half = step / 2.0
        tilt_angle = props.tilt_angle
        inverse = props.inverse_tilt

        if event.type in {'RIGHTMOUSE', 'ESC'}:
            context.area.header_text_set(None)
            self.report({'INFO'}, "Box Builder finished")
            return {'FINISHED'}

        if event.type == 'RET' and event.value == 'PRESS':
            context.area.header_text_set(None)
            self.report({'INFO'}, "Box Builder confirmed")
            return {'FINISHED'}

        # P = Stop Building (pause) – exits modal but stays in edit mode
        if event.type == 'P' and event.value == 'PRESS':
            context.area.header_text_set(None)
            self.report({'INFO'}, "Box Builder paused – select edges and click Resume Building to continue")
            return {'FINISHED'}

        if event.value != 'PRESS':
            return {'RUNNING_MODAL'}

        obj = context.edit_object
        if obj is None:
            return {'CANCELLED'}

        bm = bmesh.from_edit_mesh(obj.data)
        bm.verts.ensure_lookup_table()
        bm.edges.ensure_lookup_table()
        bm.faces.ensure_lookup_table()

        handled = False

        new_verts = []

        # --- WASD: Extrude along ring's local orientation ---
        if event.type == 'W':
            normal = self._get_ring_normal(bm)
            new_verts = self._extrude_ring(bm, normal * step)
            handled = True
        elif event.type == 'S':
            normal = self._get_ring_normal(bm)
            new_verts = self._extrude_ring(bm, -normal * step)
            handled = True
        elif event.type == 'A':
            normal = self._get_ring_normal(bm)
            right = self._get_ring_right(normal)
            new_verts = self._extrude_ring(bm, -right * step)
            handled = True
        elif event.type == 'D':
            normal = self._get_ring_normal(bm)
            right = self._get_ring_right(normal)
            new_verts = self._extrude_ring(bm, right * step)
            handled = True

        # --- QEZC: Diagonal extrude (half step each axis) ---
        elif event.type == 'Q':
            normal = self._get_ring_normal(bm)
            right = self._get_ring_right(normal)
            new_verts = self._extrude_ring(bm, normal * half + (-right) * half)
            handled = True
        elif event.type == 'E':
            normal = self._get_ring_normal(bm)
            right = self._get_ring_right(normal)
            new_verts = self._extrude_ring(bm, normal * half + right * half)
            handled = True
        elif event.type == 'Z':
            normal = self._get_ring_normal(bm)
            right = self._get_ring_right(normal)
            new_verts = self._extrude_ring(bm, -normal * half + (-right) * half)
            handled = True
        elif event.type == 'C':
            normal = self._get_ring_normal(bm)
            right = self._get_ring_right(normal)
            new_verts = self._extrude_ring(bm, -normal * half + right * half)
            handled = True

        # --- JIKL: Tilt the last ring ---
        # J = Tilt right side up, L = Tilt left side up
        # I = Tilt near side (front/+Y) up, K = Tilt far side (back/-Y) up
        # Inverse flips tilt direction
        elif event.type == 'J':
            angle = -tilt_angle if inverse else tilt_angle
            self._tilt_ring(bm, 'Y', angle)
            handled = True
        elif event.type == 'L':
            angle = tilt_angle if inverse else -tilt_angle
            self._tilt_ring(bm, 'Y', angle)
            handled = True
        elif event.type == 'I':
            angle = tilt_angle if inverse else -tilt_angle
            self._tilt_ring(bm, 'X', angle)
            handled = True
        elif event.type == 'K':
            angle = -tilt_angle if inverse else tilt_angle
            self._tilt_ring(bm, 'X', angle)
            handled = True

        # Apply size transition after any extrude
        if new_verts:
            self._apply_size_step(bm, new_verts, props)

        if handled:
            bmesh.update_edit_mesh(obj.data)
            size_info = ""
            if props.width_steps_remaining > 0:
                size_info += (f" | W={props.start_width:.3f}\u2192"
                              f"{props.end_width:.3f} ({props.width_steps_remaining} left)")
            if props.depth_steps_remaining > 0:
                size_info += (f" | D={props.start_depth:.3f}\u2192"
                              f"{props.end_depth:.3f} ({props.depth_steps_remaining} left)")
            context.area.header_text_set(
                "Box Builder | WASD=Move | QEZC=Diag | JIKL=Tilt | P=Stop | "
                f"Step={step:.3f} | Tilt={math.degrees(tilt_angle):.1f}\u00b0 | "
                f"Inverse={'ON' if inverse else 'OFF'}{size_info} | Enter/Esc=Done"
            )

        return {'RUNNING_MODAL'}

    def invoke(self, context, event):
        global _box_resume_vert_indices
        if context.edit_object is None:
            self.report({'WARNING'}, "Must be in edit mode")
            return {'CANCELLED'}

        # Pick up resume vert indices if set by the resume operator
        if _box_resume_vert_indices is not None:
            self._resume_verts = list(_box_resume_vert_indices)
            _box_resume_vert_indices = None
        else:
            self._resume_verts = None

        props = context.scene.box_builder
        context.area.header_text_set(
            "Box Builder | WASD=Move | QEZC=Diag | JIKL=Tilt | P=Stop | "
            f"Step={props.step_size:.3f} | Tilt={math.degrees(props.tilt_angle):.1f}\u00b0 | "
            f"Inverse={'ON' if props.inverse_tilt else 'OFF'} | Enter/Esc=Done"
        )
        context.window_manager.modal_handler_add(self)
        return {'RUNNING_MODAL'}

    def _get_ring_normal(self, bm):
        """Calculate the face normal of the top ring using Newell's method."""
        top_verts = self._get_top_ring_verts(bm)
        if len(top_verts) < 3:
            return Vector((0, 0, 1))

        normal = Vector((0, 0, 0))
        n = len(top_verts)
        for i in range(n):
            curr = top_verts[i].co
            next_v = top_verts[(i + 1) % n].co
            normal.x += (curr.y - next_v.y) * (curr.z + next_v.z)
            normal.y += (curr.z - next_v.z) * (curr.x + next_v.x)
            normal.z += (curr.x - next_v.x) * (curr.y + next_v.y)

        if normal.length > 0:
            normal.normalize()
        else:
            normal = Vector((0, 0, 1))
        return normal

    def _get_ring_right(self, normal):
        """Get a 'right' vector perpendicular to the ring normal."""
        up = Vector((0, 0, 1))
        if abs(normal.dot(up)) > 0.999:
            up = Vector((0, 1, 0))
        right = normal.cross(up)
        right.normalize()
        return right

    def _get_top_ring_verts(self, bm):
        """Find the active ring of vertices.

        If resuming from a selected edge, uses the stored resume verts.
        Otherwise returns the most recently created ring (highest indices).
        """
        if not bm.verts:
            return []

        bm.verts.ensure_lookup_table()

        # If we have resume verts, use those (first operation after resume)
        if self._resume_verts is not None:
            return [bm.verts[i] for i in self._resume_verts]

        # The last 4 verts added are the current square ring
        all_verts = list(bm.verts)
        if len(all_verts) < self.VERTS_PER_RING:
            return all_verts
        return all_verts[-self.VERTS_PER_RING:]

    def _extrude_ring(self, bm, offset):
        """Extrude the top square ring by the given offset vector. Returns new verts."""
        top_verts = self._get_top_ring_verts(bm)
        if not top_verts:
            return []

        # Find edges that connect top ring verts to each other
        top_set = set(top_verts)
        top_edges = []
        for e in bm.edges:
            if e.verts[0] in top_set and e.verts[1] in top_set:
                top_edges.append(e)

        # Extrude edges
        result = bmesh.ops.extrude_edge_only(bm, edges=top_edges)
        new_verts = [v for v in result['geom'] if isinstance(v, bmesh.types.BMVert)]

        # Move new verts by offset
        for v in new_verts:
            v.co += offset

        # Clear resume state – new verts are now at the end of the vert list
        self._resume_verts = None
        return new_verts

    def _apply_size_step(self, bm, new_verts, props):
        """Scale the newly extruded ring if a width/depth transition is active.

        Unlike a cylinder (uniform radius scaling), a box scales independently
        along its local width (right vector) and depth (up vector relative to
        the ring normal).  We project each vert onto those axes relative to the
        ring center and apply per-axis scale factors.
        """
        if not new_verts:
            return

        width_active = (props.width_steps_remaining > 0
                        and abs(props.end_width - props.start_width) > 1e-6)
        depth_active = (props.depth_steps_remaining > 0
                        and abs(props.end_depth - props.start_depth) > 1e-6)

        if not width_active and not depth_active:
            return

        # Calculate center of the new ring
        center = Vector((0, 0, 0))
        for v in new_verts:
            center += v.co
        center /= len(new_verts)

        # Determine local axes of the ring
        normal = self._get_ring_normal(bm)
        right = self._get_ring_right(normal)
        up = right.cross(normal)
        up.normalize()

        # Compute scale factors
        w_scale = 1.0
        d_scale = 1.0

        if width_active:
            w_inc = (props.end_width - props.start_width) / props.width_steps_remaining
            new_w = props.start_width + w_inc
            w_scale = new_w / props.start_width
            props.start_width = new_w
            props.width_steps_remaining -= 1

        if depth_active:
            d_inc = (props.end_depth - props.start_depth) / props.depth_steps_remaining
            new_d = props.start_depth + d_inc
            d_scale = new_d / props.start_depth
            props.start_depth = new_d
            props.depth_steps_remaining -= 1

        # Apply per-axis scaling
        for v in new_verts:
            offset = v.co - center
            r_comp = offset.dot(right)
            u_comp = offset.dot(up)
            n_comp = offset.dot(normal)
            v.co = center + right * (r_comp * w_scale) + up * (u_comp * d_scale) + normal * n_comp

    def _tilt_ring(self, bm, axis, angle):
        """Tilt the top ring around its center by angle (radians) on the given axis."""
        top_verts = self._get_top_ring_verts(bm)
        if not top_verts:
            return

        # Calculate center of the ring
        center = Vector((0, 0, 0))
        for v in top_verts:
            center += v.co
        center /= len(top_verts)

        # Build rotation matrix
        if axis == 'X':
            rot_matrix = Matrix.Rotation(angle, 3, 'X')
        elif axis == 'Y':
            rot_matrix = Matrix.Rotation(angle, 3, 'Y')
        else:
            rot_matrix = Matrix.Rotation(angle, 3, 'Z')

        # Rotate each vert around the ring center
        for v in top_verts:
            local = v.co - center
            rotated = rot_matrix @ local
            v.co = center + rotated


class BOXBUILDER_OT_resume(bpy.types.Operator):
    """Resume building from selected edges"""
    bl_idname = "mesh.box_builder_resume"
    bl_label = "Resume Box Builder"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        global _box_resume_vert_indices

        obj = context.edit_object
        if obj is None:
            self.report({'WARNING'}, "Must be in edit mode")
            return {'CANCELLED'}

        bm = bmesh.from_edit_mesh(obj.data)
        bm.edges.ensure_lookup_table()

        # Collect verts from selected edges
        selected_edges = [e for e in bm.edges if e.select]
        if not selected_edges:
            self.report({'WARNING'}, "Select the edges of the ring you want to resume from")
            return {'CANCELLED'}

        vert_indices = set()
        for e in selected_edges:
            vert_indices.add(e.verts[0].index)
            vert_indices.add(e.verts[1].index)

        # Store for the modal to pick up
        _box_resume_vert_indices = sorted(vert_indices)

        # Launch the modal operator
        bpy.ops.mesh.box_builder_modal('INVOKE_DEFAULT')
        return {'FINISHED'}


class BOXBUILDER_PT_panel(bpy.types.Panel):
    """Sidebar panel for Box Builder settings"""
    bl_label = "Box Builder"
    bl_idname = "BOXBUILDER_PT_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Box Builder"

    def draw(self, context):
        layout = self.layout
        props = context.scene.box_builder

        layout.label(text="Square Settings:")
        layout.prop(props, "width")
        layout.prop(props, "depth")

        layout.separator()
        layout.label(text="Build Settings:")
        layout.prop(props, "step_size")
        layout.prop(props, "tilt_angle_degrees")
        layout.prop(props, "inverse_tilt")

        layout.separator()
        layout.label(text="Width Transition:")
        row = layout.row()
        row.prop(props, "start_width")
        row.enabled = False  # Start width is read-only
        layout.prop(props, "end_width")
        layout.prop(props, "width_steps")
        if props.width_steps_remaining > 0:
            layout.label(text=f"Width steps remaining: {props.width_steps_remaining}")

        layout.separator()
        layout.label(text="Depth Transition:")
        row = layout.row()
        row.prop(props, "start_depth")
        row.enabled = False  # Start depth is read-only
        layout.prop(props, "end_depth")
        layout.prop(props, "depth_steps")
        if props.depth_steps_remaining > 0:
            layout.label(text=f"Depth steps remaining: {props.depth_steps_remaining}")

        layout.separator()
        layout.operator("mesh.box_builder_start", text="Start Building", icon='MESH_CUBE')
        layout.operator("mesh.box_builder_resume", text="Resume Building", icon='PLAY')

        layout.separator()
        layout.label(text="Controls (while building):")
        box = layout.box()
        col = box.column(align=True)
        col.label(text="W = Extrude Up")
        col.label(text="S = Extrude Down")
        col.label(text="A = Extrude Left")
        col.label(text="D = Extrude Right")
        col.separator()
        col.label(text="Q = Diagonal Up-Left")
        col.label(text="E = Diagonal Up-Right")
        col.label(text="Z = Diagonal Down-Left")
        col.label(text="C = Diagonal Down-Right")
        col.separator()
        col.label(text="J = Tilt Right Side Up")
        col.label(text="L = Tilt Left Side Up")
        col.label(text="I = Tilt Near Side Up")
        col.label(text="K = Tilt Far Side Up")
        col.separator()
        col.label(text="Inverse: Reverses JIKL")
        col.separator()
        col.label(text="P = Stop Building (pause)")
        col.label(text="Enter / Esc = Finish")


def _on_end_width_update(self, context):
    """Reset remaining width steps when end width changes."""
    if abs(self.end_width - self.start_width) > 1e-6:
        self.width_steps_remaining = self.width_steps
    else:
        self.width_steps_remaining = 0


def _on_width_steps_update(self, context):
    """Reset remaining width steps when step count changes."""
    if abs(self.end_width - self.start_width) > 1e-6:
        self.width_steps_remaining = self.width_steps
    else:
        self.width_steps_remaining = 0


def _on_end_depth_update(self, context):
    """Reset remaining depth steps when end depth changes."""
    if abs(self.end_depth - self.start_depth) > 1e-6:
        self.depth_steps_remaining = self.depth_steps
    else:
        self.depth_steps_remaining = 0


def _on_depth_steps_update(self, context):
    """Reset remaining depth steps when step count changes."""
    if abs(self.end_depth - self.start_depth) > 1e-6:
        self.depth_steps_remaining = self.depth_steps
    else:
        self.depth_steps_remaining = 0


class BoxBuilderProperties(bpy.types.PropertyGroup):
    width: FloatProperty(
        name="Width",
        description="Width of the starting square (X axis)",
        default=1.0,
        min=0.01,
        max=100.0,
        unit='LENGTH',
    )
    depth: FloatProperty(
        name="Depth",
        description="Depth of the starting square (Y axis)",
        default=1.0,
        min=0.01,
        max=100.0,
        unit='LENGTH',
    )
    step_size: FloatProperty(
        name="Step Size",
        description="Distance to extrude per keypress",
        default=0.1,
        min=0.001,
        max=10.0,
        unit='LENGTH',
    )
    tilt_angle_degrees: FloatProperty(
        name="Tilt Angle",
        description="Degrees to tilt per keypress",
        default=5.0,
        min=0.1,
        max=45.0,
        subtype='ANGLE',
    )
    inverse_tilt: BoolProperty(
        name="Inverse Tilt",
        description="When enabled, JIKL tilts in the opposite direction (down instead of up)",
        default=False,
    )
    start_width: FloatProperty(
        name="Start Width",
        description="Current width of the building ring (auto-updated)",
        default=1.0,
        min=0.01,
        max=100.0,
        unit='LENGTH',
    )
    end_width: FloatProperty(
        name="End Width",
        description="Target width to transition toward",
        default=1.0,
        min=0.01,
        max=100.0,
        unit='LENGTH',
        update=_on_end_width_update,
    )
    width_steps: IntProperty(
        name="Width Steps",
        description="Number of extrude presses to reach the end width",
        default=5,
        min=1,
        max=100,
        update=_on_width_steps_update,
    )
    width_steps_remaining: IntProperty(
        name="Width Steps Left",
        description="Remaining extrude presses until end width is reached",
        default=0,
        min=0,
    )
    start_depth: FloatProperty(
        name="Start Depth",
        description="Current depth of the building ring (auto-updated)",
        default=1.0,
        min=0.01,
        max=100.0,
        unit='LENGTH',
    )
    end_depth: FloatProperty(
        name="End Depth",
        description="Target depth to transition toward",
        default=1.0,
        min=0.01,
        max=100.0,
        unit='LENGTH',
        update=_on_end_depth_update,
    )
    depth_steps: IntProperty(
        name="Depth Steps",
        description="Number of extrude presses to reach the end depth",
        default=5,
        min=1,
        max=100,
        update=_on_depth_steps_update,
    )
    depth_steps_remaining: IntProperty(
        name="Depth Steps Left",
        description="Remaining extrude presses until end depth is reached",
        default=0,
        min=0,
    )

    @property
    def tilt_angle(self):
        """Return tilt angle in radians."""
        return math.radians(self.tilt_angle_degrees)


classes = (
    BoxBuilderProperties,
    BOXBUILDER_OT_start,
    BOXBUILDER_OT_modal,
    BOXBUILDER_OT_resume,
    BOXBUILDER_PT_panel,
)


def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.Scene.box_builder = bpy.props.PointerProperty(type=BoxBuilderProperties)


def unregister():
    del bpy.types.Scene.box_builder
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)


if __name__ == "__main__":
    register()
