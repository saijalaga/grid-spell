using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGridLayout : LayoutGroup
{
    public int rows = 4;
    public int columns = 5;

    public Vector2 spacing = new Vector2(10, 10);
    public Vector2 cardSize;

    public int preferredTopPadding = 10;

    public override void CalculateLayoutInputVertical()
    {
        if (rows <= 0 || columns <= 0)
        {
            rows = 4;
            columns = 5;
        }

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        // Calculate card size based on height first
        float cardHeight = (parentHeight - 2 * preferredTopPadding - spacing.y * (rows - 1)) / rows;
        float cardWidth = cardHeight;

        // If cards + spacing are too wide -> recalc by width instead
        if (cardWidth * columns + spacing.x * (columns - 1) > parentWidth)
        {
            cardWidth = (parentWidth - 2 * preferredTopPadding - (columns - 1) * spacing.x) / columns;
            cardHeight = cardWidth;
        }

        cardSize = new Vector2(cardWidth, cardHeight);

        // Padding (centered in parent)
        padding.left = Mathf.FloorToInt((parentWidth - columns * cardWidth - spacing.x * (columns - 1)) / 2);
        padding.top = Mathf.FloorToInt((parentHeight - rows * cardHeight - spacing.y * (rows - 1)) / 2);
        padding.bottom = padding.top;

        // Place each child
        for (int i = 0; i < rectChildren.Count; i++)
        {
            int rowCount = i / columns;
            int columnCount = i % columns;

            var item = rectChildren[i];

            float xPos = padding.left + (cardSize.x + spacing.x) * columnCount;
            float yPos = padding.top + (cardSize.y + spacing.y) * rowCount;

            SetChildAlongAxis(item, 0, xPos, cardSize.x);
            SetChildAlongAxis(item, 1, yPos, cardSize.y);
        }
    }

    public override void SetLayoutHorizontal() { }
    public override void SetLayoutVertical() { }
}
