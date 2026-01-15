// This custom toolbox item can be added to the Tool box by including line 12 and line 13 as well as line 4
// then going to Tools > Command Line > Developer Command Line and typing csc /target:library <filename.cs> to make the DLL needed.
// Make sure you are on the Designer tab so the toolbox will populate then Right Click in the toolbox and select "Choose Items"
// Click "Browse" and Navigate to your control and then click OK.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SA_ToolBelt
{
    public class HorizontalCheckedListBox : CheckedListBox
    {
        private const int ITEM_PADDING = 8; // Padding between items
        private const int CHECKBOX_TEXT_SPACING = 6; // Space between checkbox and text
        private const int MIN_ITEM_WIDTH = 120; // Minimum width for each item

        public HorizontalCheckedListBox()
        {
            this.MultiColumn = true;
            this.HorizontalScrollbar = true;
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.ItemHeight = 24; // Slightly increased for better vertical spacing
            this.ColumnWidth = MIN_ITEM_WIDTH;

            this.Resize += (s, e) => UpdateColumnWidth();
        }

        private void UpdateColumnWidth()
        {
            // Calculate maximum text width
            int maxWidth = MIN_ITEM_WIDTH;
            using (Graphics g = this.CreateGraphics())
            {
                foreach (var item in Items)
                {
                    int textWidth = (int)g.MeasureString(item.ToString(), this.Font).Width;
                    maxWidth = Math.Max(maxWidth, textWidth + 25); // 25 pixels for checkbox and padding
                }
            }

            // Calculate how many columns can fit
            int availableWidth = Width - SystemInformation.VerticalScrollBarWidth;
            int itemsPerRow = Math.Max(1, availableWidth / (maxWidth + ITEM_PADDING));

            // Set column width
            base.ColumnWidth = Math.Max(MIN_ITEM_WIDTH, (availableWidth - (ITEM_PADDING * (itemsPerRow - 1))) / itemsPerRow);
        }

        public void AddItemSorted(object item)
        {
            List<object> items = Items.Cast<object>().ToList();
            items.Add(item);
            items.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase));
            Items.Clear();
            Items.AddRange(items.ToArray());
            UpdateColumnWidth(); // Update column width after adding items
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || Items.Count == 0 || e.Index >= Items.Count)
                return;

            e.DrawBackground();

            bool isChecked = GetItemChecked(e.Index);
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            string text = Items[e.Index].ToString();

            // Calculate bounds with proper spacing
            Rectangle bounds = e.Bounds;

            // Checkbox bounds with vertical centering
            Rectangle checkBoxBounds = new Rectangle(
                bounds.X + ITEM_PADDING,
                bounds.Y + ((bounds.Height - 13) / 2),
                13,
                13
            );

            // Text bounds with proper spacing
            Rectangle textBounds = new Rectangle(
                checkBoxBounds.Right + CHECKBOX_TEXT_SPACING,
                bounds.Y,
                bounds.Width - checkBoxBounds.Width - CHECKBOX_TEXT_SPACING - (ITEM_PADDING * 2),
                bounds.Height
            );

            // Draw checkbox
            ControlPaint.DrawCheckBox(
                e.Graphics,
                checkBoxBounds,
                isChecked ? ButtonState.Checked : ButtonState.Normal
            );

            // Draw text
            Color textColor = isSelected ? SystemColors.HighlightText : SystemColors.ControlText;
            using (Brush brush = new SolidBrush(textColor))
            {
                StringFormat format = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                e.Graphics.DrawString(
                    text,
                    this.Font,
                    brush,
                    textBounds,
                    format
                );
            }

            if (isSelected)
                e.DrawFocusRectangle();
        }
    }
}