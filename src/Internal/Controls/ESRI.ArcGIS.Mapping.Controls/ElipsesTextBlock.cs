/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Mapping.Controls
{
  /// <summary>
  /// Implements a custom textblock that truncates the text and adds "..." if it
  /// doesn't fit in the alloted space.
  /// </summary>
  public class EllipsesTextBlock : ContentControl
  {
    /// <summary>
    /// A TextBlock that gets set as the control's content and is ultimately the control 
    /// that displays the text
    /// </summary>
    TextBlock _textBlock;

    #region Text (DependencyProperty)

    /// <summary>
    /// Gets or sets the Text DependencyProperty. This is the text that will be displayed.
    /// </summary>
    public string Text
    {
      get { return (string)GetValue(TextProperty); }
      set { SetValue(TextProperty, value); }
    }
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(EllipsesTextBlock),
        new PropertyMetadata(null, new PropertyChangedCallback(OnTextChanged)));

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((EllipsesTextBlock)d).OnTextChanged(e);
    }

    protected virtual void OnTextChanged(DependencyPropertyChangedEventArgs e)
    {
      InvalidateMeasure();
    }

    #endregion

    #region TextWrapping (DependencyProperty)

    /// <summary>
    /// Gets or sets the TextWrapping property. This corresponds to TextBlock.TextWrapping.
    /// </summary>
    public TextWrapping TextWrapping
    {
      get { return (TextWrapping)GetValue(TextWrappingProperty); }
      set { SetValue(TextWrappingProperty, value); }
    }
    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(EllipsesTextBlock),
        new PropertyMetadata(TextWrapping.NoWrap, new PropertyChangedCallback(OnTextWrappingChanged)));

    private static void OnTextWrappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((EllipsesTextBlock)d).OnTextWrappingChanged(e);
    }

    protected virtual void OnTextWrappingChanged(DependencyPropertyChangedEventArgs e)
    {
      _textBlock.TextWrapping = (TextWrapping)e.NewValue;
      InvalidateMeasure();
    }

    #endregion

    #region LineHeight (DependencyProperty)

    /// <summary>
    /// Gets or sets the LineHeight property. This property corresponds to TextBlock.LineHeight;
    /// </summary>
    public double LineHeight
    {
      get { return (double)GetValue(LineHeightProperty); }
      set { SetValue(LineHeightProperty, value); }
    }

    public static readonly DependencyProperty LineHeightProperty =
        DependencyProperty.Register("LineHeight", typeof(double), typeof(EllipsesTextBlock),
        new PropertyMetadata(0.0, new PropertyChangedCallback(OnLineHeightChanged)));

    private static void OnLineHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((EllipsesTextBlock)d).OnLineHeightChanged(e);
    }

    protected virtual void OnLineHeightChanged(DependencyPropertyChangedEventArgs e)
    {
      _textBlock.LineHeight = LineHeight;
      InvalidateMeasure();
    }

    #endregion

    #region LineStackingStrategy (DependencyProperty)

    /// <summary>
    /// Gets or sets the LineStackingStrategy DependencyProperty. This corresponds to TextBlock.LineStackingStrategy.
    /// </summary>
    public LineStackingStrategy LineStackingStrategy
    {
      get { return (LineStackingStrategy)GetValue(LineStackingStrategyProperty); }
      set { SetValue(LineStackingStrategyProperty, value); }
    }
    public static readonly DependencyProperty LineStackingStrategyProperty =
        DependencyProperty.Register("LineStackingStrategy", typeof(LineStackingStrategy), typeof(EllipsesTextBlock),
        new PropertyMetadata(LineStackingStrategy.BlockLineHeight, new PropertyChangedCallback(OnLineStackingStrategyChanged)));

    private static void OnLineStackingStrategyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((EllipsesTextBlock)d).OnLineStackingStrategyChanged(e);
    }

    protected virtual void OnLineStackingStrategyChanged(DependencyPropertyChangedEventArgs e)
    {
      _textBlock.LineStackingStrategy = (LineStackingStrategy)e.NewValue;
      InvalidateMeasure();
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the EllipsesTextBlock class
    /// </summary>
    public EllipsesTextBlock()
    {
      // create our textBlock and initialize
      _textBlock = new TextBlock();
      Content = _textBlock;
    }

    /// <summary>
    /// Handles the measure part of the measure and arrange layout process. During this process,
    /// measure the textBlock as content with increasingly smaller amounts of text until we find text that fits.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
      // just to make the code easier to read
      bool wrapping = TextWrapping == TextWrapping.Wrap;

      Size unboundSize = wrapping ? new Size(availableSize.Width, double.PositiveInfinity) : new Size(double.PositiveInfinity, availableSize.Height);
      string reducedText = Text;

      // set the text and measure it to see if it fits without alteration
      _textBlock.Text = reducedText;
      Size textSize = base.MeasureOverride(unboundSize);

      while (wrapping ? textSize.Height > availableSize.Height : textSize.Width > availableSize.Width)
      {
        int prevLength = reducedText.Length;
        reducedText = ReduceText(reducedText);

        if (reducedText.Length == prevLength)
        {
          break;
        }

        _textBlock.Text = reducedText + "...";
        textSize = base.MeasureOverride(unboundSize);
      }

      return base.MeasureOverride(availableSize);
    }

    /// <summary>
    /// Reduces the length of the text.
    /// </summary>
    protected virtual string ReduceText(string text)
    {
      if (text.Length >= 3)
        return text.Substring(0, text.Length - 3);
      else
        return "";
    }
  }
}
