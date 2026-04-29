bl_info = {
    "name": "Simple Bevel",
    "author": "SA_Toolbelt",
    "version": (1, 0, 0),
    "blender": (3, 0, 0),
    "location": "View3D > Sidebar > Simple Bevel",
    "description": "Select an edge and press +/- to round it. That's it.",
    "category": "Mesh",
}

import bpy
import bmesh
import math
from bpy.props import FloatProperty, IntProperty


class SIMPLEBEVEL_OT_modal(bpy.types.Operator):
    """Select edges then use +/- to bevel them"""
    bl_idname = "mesh.simple_bevel_start"
    bl_label = "Simple Bevel"
    bl_options = {'REGISTER', 'UNDO'}

    def invoke(self, context, event):
        obj = context.edit_object
        if obj is None:
            self.report({'WARNING'}, "Must be in edit mode")
            return {'CANCELLED'}

        # Check that we're in edge select mode
        if context.tool_settings.mesh_select_mode[1] is False:
            self.report({'WARNING'}, "Switch to edge select mode first")
            return {'CANCELLED'}

        # Check that at least one edge is selected
        bm = bmesh.from_edit_mesh(obj.data)
        selected_edges = [e for e in bm.edges if e.select]
        if not selected_edges:
            self.report({'WARNING'}, "Select at least one edge first")
            return {'CANCELLED'}

        props = context.scene.simple_bevel
        self._width = props.initial_width
        self._segments = props.initial_segments
        self._update_header(context)

        context.window_manager.modal_handler_add(self)
        return {'RUNNING_MODAL'}

    def modal(self, context, event):
        props = context.scene.simple_bevel

        if event.type in {'RIGHTMOUSE', 'ESC'}:
            context.area.header_text_set(None)
            self.report({'INFO'}, "Simple Bevel cancelled")
            return {'FINISHED'}

        if event.type == 'RET' and event.value == 'PRESS':
            context.area.header_text_set(None)
            self.report({'INFO'}, "Simple Bevel confirmed")
            return {'FINISHED'}

        if event.value != 'PRESS':
            return {'RUNNING_MODAL'}

        obj = context.edit_object
        if obj is None:
            return {'CANCELLED'}

        handled = False

        # + / = key: increase bevel (apply one step wider)
        if event.type in {'NUMPAD_PLUS', 'EQUAL'}:
            self._width += props.width_step
            self._apply_bevel(context)
            handled = True

        # - key: decrease bevel
        elif event.type in {'NUMPAD_MINUS', 'MINUS'}:
            self._width = max(0.001, self._width - props.width_step)
            self._apply_bevel(context)
            handled = True

        # [ key: fewer segments (less round)
        elif event.type == 'LEFT_BRACKET':
            self._segments = max(1, self._segments - 1)
            self._apply_bevel(context)
            handled = True

        # ] key: more segments (rounder)
        elif event.type == 'RIGHT_BRACKET':
            self._segments = min(20, self._segments + 1)
            self._apply_bevel(context)
            handled = True

        if handled:
            self._update_header(context)

        return {'RUNNING_MODAL'}

    def _apply_bevel(self, context):
        """Undo the last bevel and re-apply with current settings."""
        # Undo previous bevel if we already applied one
        if hasattr(self, '_has_applied') and self._has_applied:
            bpy.ops.ed.undo()

        obj = context.edit_object
        bm = bmesh.from_edit_mesh(obj.data)

        selected_edges = [e for e in bm.edges if e.select]
        if not selected_edges:
            return

        try:
            result = bmesh.ops.bevel(
                bm,
                geom=selected_edges,
                offset=self._width,
                offset_type='OFFSET',
                segments=self._segments,
                profile=0.5,
                affect='EDGES',
            )
            bmesh.update_edit_mesh(obj.data)
            self._has_applied = True
        except Exception as e:
            self.report({'WARNING'}, f"Bevel failed: {e}")

    def _update_header(self, context):
        context.area.header_text_set(
            f"Simple Bevel | Width={self._width:.4f} | "
            f"Segments={self._segments} | "
            "+/-=Width | [/]=Segments | Enter=Confirm | Esc=Cancel"
        )


class SIMPLEBEVEL_PT_panel(bpy.types.Panel):
    """Sidebar panel for Simple Bevel settings"""
    bl_label = "Simple Bevel"
    bl_idname = "SIMPLEBEVEL_PT_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Simple Bevel"

    def draw(self, context):
        layout = self.layout
        props = context.scene.simple_bevel

        layout.label(text="Starting Values:")
        layout.prop(props, "initial_width")
        layout.prop(props, "initial_segments")
        layout.prop(props, "width_step")

        layout.separator()
        layout.operator("mesh.simple_bevel_start", text="Bevel Selected Edges", icon='MOD_BEVEL')

        layout.separator()
        layout.label(text="Controls (while beveling):")
        box = layout.box()
        col = box.column(align=True)
        col.label(text="+ / - = Adjust width")
        col.label(text="[ / ] = Adjust segments")
        col.label(text="Enter = Confirm")
        col.label(text="Esc = Cancel")


class SimpleBevelProperties(bpy.types.PropertyGroup):
    initial_width: FloatProperty(
        name="Initial Width",
        description="Starting bevel width",
        default=0.01,
        min=0.001,
        max=10.0,
        unit='LENGTH',
    )
    initial_segments: IntProperty(
        name="Initial Segments",
        description="Starting number of bevel segments (more = rounder)",
        default=4,
        min=1,
        max=20,
    )
    width_step: FloatProperty(
        name="Width Step",
        description="How much width changes per +/- keypress",
        default=0.01,
        min=0.001,
        max=1.0,
        unit='LENGTH',
    )


classes = (
    SimpleBevelProperties,
    SIMPLEBEVEL_OT_modal,
    SIMPLEBEVEL_PT_panel,
)


def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.Scene.simple_bevel = bpy.props.PointerProperty(type=SimpleBevelProperties)


def unregister():
    del bpy.types.Scene.simple_bevel
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)


if __name__ == "__main__":
    register()
