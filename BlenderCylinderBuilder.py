bl_info = {
    "name": "Interactive Cylinder Builder",
    "author": "SA_Toolbelt",
    "version": (1, 0, 0),
    "blender": (3, 0, 0),
    "location": "View3D > Sidebar > Cylinder Builder",
    "description": "Interactively build custom cylinders by extruding and tilting rings with WASD/JIKL keys",
    "category": "Mesh",
}

import bpy
import bmesh
import math
import mathutils
from mathutils import Vector, Matrix
from bpy.props import FloatProperty, IntProperty, BoolProperty


# Class-level storage for resume vert indices (set by resume operator, read by modal)
_cyl_resume_vert_indices = None


class CYLBUILDER_OT_start(bpy.types.Operator):
    """Start interactive cylinder building mode"""
    bl_idname = "mesh.cylinder_builder_start"
    bl_label = "Start Cylinder Builder"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        props = context.scene.cyl_builder

        # Create the starting circle mesh
        mesh = bpy.data.meshes.new("CylinderBuilder")
        obj = bpy.data.objects.new("CylinderBuilder", mesh)
        context.collection.objects.link(obj)

        # Deselect all, then select and activate the new object
        bpy.ops.object.select_all(action='DESELECT')
        obj.select_set(True)
        context.view_layer.objects.active = obj

        # Build initial circle with bmesh
        bm = bmesh.new()
        verts = []
        segments = props.circle_segments
        radius = props.circle_radius
        # Initialize radius transition to match starting radius
        props.start_radius = radius
        props.end_radius = radius
        props.radius_steps_remaining = 0
        for i in range(segments):
            angle = 2 * math.pi * i / segments
            x = radius * math.cos(angle)
            y = radius * math.sin(angle)
            v = bm.verts.new((x, y, 0.0))
            verts.append(v)

        # Create edges forming the circle
        for i in range(segments):
            bm.edges.new((verts[i], verts[(i + 1) % segments]))

        # Create the face
        bm.faces.new(verts)

        bm.to_mesh(mesh)
        bm.free()
        mesh.update()

        # Switch to edit mode for the modal operator
        bpy.ops.object.mode_set(mode='EDIT')

        # Launch the modal operator
        bpy.ops.mesh.cylinder_builder_modal('INVOKE_DEFAULT')
        return {'FINISHED'}


class CYLBUILDER_OT_modal(bpy.types.Operator):
    """Modal operator that captures keypresses for cylinder building"""
    bl_idname = "mesh.cylinder_builder_modal"
    bl_label = "Cylinder Builder Modal"
    bl_options = {'REGISTER', 'UNDO'}

    def modal(self, context, event):
        props = context.scene.cyl_builder
        step = props.step_size
        half = step / 2.0
        tilt_angle = props.tilt_angle
        inverse = props.inverse_tilt

        if event.type in {'RIGHTMOUSE', 'ESC'}:
            context.area.header_text_set(None)
            self.report({'INFO'}, "Cylinder Builder finished")
            return {'FINISHED'}

        if event.type == 'RET' and event.value == 'PRESS':
            context.area.header_text_set(None)
            self.report({'INFO'}, "Cylinder Builder confirmed")
            return {'FINISHED'}

        # P = Stop Building (pause) – exits modal but stays in edit mode
        if event.type == 'P' and event.value == 'PRESS':
            context.area.header_text_set(None)
            self.report({'INFO'}, "Cylinder Builder paused – select edges and click Resume Building to continue")
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
            # Tilt right side up = rotate around Y axis (positive)
            angle = -tilt_angle if inverse else tilt_angle
            self._tilt_ring(bm, 'Y', angle)
            handled = True
        elif event.type == 'L':
            # Tilt left side up = rotate around Y axis (negative)
            angle = tilt_angle if inverse else -tilt_angle
            self._tilt_ring(bm, 'Y', angle)
            handled = True
        elif event.type == 'I':
            # Tilt near side up = rotate around X axis (negative)
            angle = tilt_angle if inverse else -tilt_angle
            self._tilt_ring(bm, 'X', angle)
            handled = True
        elif event.type == 'K':
            # Tilt far side up = rotate around X axis (positive)
            angle = -tilt_angle if inverse else tilt_angle
            self._tilt_ring(bm, 'X', angle)
            handled = True

        # Apply radius transition after any extrude
        if new_verts:
            self._apply_radius_step(new_verts, props)

        if handled:
            bmesh.update_edit_mesh(obj.data)
            radius_info = ""
            if props.radius_steps_remaining > 0:
                radius_info = (f" | Radius={props.start_radius:.3f}→"
                               f"{props.end_radius:.3f} ({props.radius_steps_remaining} left)")
            context.area.header_text_set(
                "Cylinder Builder | WASD=Move | JIKL=Tilt | P=Stop | "
                f"Step={step:.3f} | Tilt={math.degrees(tilt_angle):.1f}° | "
                f"Inverse={'ON' if inverse else 'OFF'}{radius_info} | Enter/Esc=Done"
            )

        return {'RUNNING_MODAL'}

    def invoke(self, context, event):
        global _cyl_resume_vert_indices
        if context.edit_object is None:
            self.report({'WARNING'}, "Must be in edit mode")
            return {'CANCELLED'}

        # Pick up resume vert indices if set by the resume operator
        if _cyl_resume_vert_indices is not None:
            self._resume_verts = list(_cyl_resume_vert_indices)
            _cyl_resume_vert_indices = None
        else:
            self._resume_verts = None

        props = context.scene.cyl_builder
        context.area.header_text_set(
            "Cylinder Builder | WASD=Move | JIKL=Tilt | P=Stop | "
            f"Step={props.step_size:.3f} | Tilt={math.degrees(props.tilt_angle):.1f}° | "
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

        segments = bpy.context.scene.cyl_builder.circle_segments

        # The last N verts added are the current ring
        all_verts = list(bm.verts)
        if len(all_verts) < segments:
            return all_verts
        return all_verts[-segments:]

    def _extrude_ring(self, bm, offset):
        """Extrude the top ring by the given offset vector. Returns new verts."""
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

    def _apply_radius_step(self, new_verts, props):
        """Scale the newly extruded ring if a radius transition is active."""
        if props.radius_steps_remaining <= 0:
            return
        if abs(props.end_radius - props.start_radius) < 1e-6:
            return
        if not new_verts:
            return

        increment = (props.end_radius - props.start_radius) / props.radius_steps_remaining
        new_radius = props.start_radius + increment

        # Calculate center of the new ring
        center = Vector((0, 0, 0))
        for v in new_verts:
            center += v.co
        center /= len(new_verts)

        # Scale each vert's distance from center
        scale_factor = new_radius / props.start_radius
        for v in new_verts:
            direction = v.co - center
            v.co = center + direction * scale_factor

        props.start_radius = new_radius
        props.radius_steps_remaining -= 1

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


class CYLBUILDER_OT_resume(bpy.types.Operator):
    """Resume building from selected edges"""
    bl_idname = "mesh.cylinder_builder_resume"
    bl_label = "Resume Cylinder Builder"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        global _cyl_resume_vert_indices

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
        _cyl_resume_vert_indices = sorted(vert_indices)

        # Launch the modal operator
        bpy.ops.mesh.cylinder_builder_modal('INVOKE_DEFAULT')
        return {'FINISHED'}


class CYLBUILDER_PT_panel(bpy.types.Panel):
    """Sidebar panel for Cylinder Builder settings"""
    bl_label = "Cylinder Builder"
    bl_idname = "CYLBUILDER_PT_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Cyl Builder"

    def draw(self, context):
        layout = self.layout
        props = context.scene.cyl_builder

        layout.label(text="Circle Settings:")
        layout.prop(props, "circle_segments")
        layout.prop(props, "circle_radius")

        layout.separator()
        layout.label(text="Build Settings:")
        layout.prop(props, "step_size")
        layout.prop(props, "tilt_angle_degrees")
        layout.prop(props, "inverse_tilt")

        layout.separator()
        layout.label(text="Radius Transition:")
        row = layout.row()
        row.prop(props, "start_radius")
        row.enabled = False  # Start radius is read-only
        layout.prop(props, "end_radius")
        layout.prop(props, "radius_steps")
        if props.radius_steps_remaining > 0:
            layout.label(text=f"Steps remaining: {props.radius_steps_remaining}")

        layout.separator()
        layout.operator("mesh.cylinder_builder_start", text="Start Building", icon='MESH_CYLINDER')
        layout.operator("mesh.cylinder_builder_resume", text="Resume Building", icon='PLAY')

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


def _on_end_radius_update(self, context):
    """Reset remaining steps when end radius changes."""
    if abs(self.end_radius - self.start_radius) > 1e-6:
        self.radius_steps_remaining = self.radius_steps
    else:
        self.radius_steps_remaining = 0


def _on_radius_steps_update(self, context):
    """Reset remaining steps when step count changes."""
    if abs(self.end_radius - self.start_radius) > 1e-6:
        self.radius_steps_remaining = self.radius_steps
    else:
        self.radius_steps_remaining = 0


class CylBuilderProperties(bpy.types.PropertyGroup):
    circle_segments: IntProperty(
        name="Segments",
        description="Number of segments in the circle",
        default=32,
        min=3,
        max=256,
    )
    circle_radius: FloatProperty(
        name="Radius",
        description="Radius of the starting circle",
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
    start_radius: FloatProperty(
        name="Start Radius",
        description="Current radius of the building ring (auto-updated)",
        default=1.0,
        min=0.01,
        max=100.0,
        unit='LENGTH',
    )
    end_radius: FloatProperty(
        name="End Radius",
        description="Target radius to transition toward",
        default=1.0,
        min=0.01,
        max=100.0,
        unit='LENGTH',
        update=_on_end_radius_update,
    )
    radius_steps: IntProperty(
        name="Radius Steps",
        description="Number of extrude presses to reach the end radius",
        default=5,
        min=1,
        max=100,
        update=_on_radius_steps_update,
    )
    radius_steps_remaining: IntProperty(
        name="Steps Left",
        description="Remaining extrude presses until end radius is reached",
        default=0,
        min=0,
    )

    @property
    def tilt_angle(self):
        """Return tilt angle in radians."""
        return math.radians(self.tilt_angle_degrees)


classes = (
    CylBuilderProperties,
    CYLBUILDER_OT_start,
    CYLBUILDER_OT_modal,
    CYLBUILDER_OT_resume,
    CYLBUILDER_PT_panel,
)


def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.Scene.cyl_builder = bpy.props.PointerProperty(type=CylBuilderProperties)


def unregister():
    del bpy.types.Scene.cyl_builder
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)


if __name__ == "__main__":
    register()
