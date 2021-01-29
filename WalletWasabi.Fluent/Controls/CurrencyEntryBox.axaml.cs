using System;
using Avalonia;
using Avalonia.Controls;

namespace WalletWasabi.Fluent.Controls
{
	public class CurrencyEntryBox : TextBox
	{
		public static readonly StyledProperty<string> ConversionProperty =
			AvaloniaProperty.Register<CurrencyEntryBox, string>(nameof(Conversion));

		public static readonly StyledProperty<decimal> ConversionRateProperty =
			AvaloniaProperty.Register<CurrencyEntryBox, decimal>(nameof(ConversionRate));

		public static readonly StyledProperty<string> CurrencyCodeProperty =
			AvaloniaProperty.Register<CurrencyEntryBox, string>(nameof(CurrencyCode));

		public static readonly StyledProperty<string> ConversionCurrencyCodeProperty =
			AvaloniaProperty.Register<CurrencyEntryBox, string>(nameof(ConversionCurrencyCode));

		public CurrencyEntryBox()
		{
			this.GetObservable(TextProperty).Subscribe(x =>
			{
				if (decimal.TryParse(x, out var result) && ConversionRate > 0)
				{
					Conversion = $"≈ {result * ConversionRate :N}"
					             + (!string.IsNullOrWhiteSpace(ConversionCurrencyCode) ? $" {ConversionCurrencyCode}" : "");
				}
				else
				{
					Conversion = string.Empty;
				}
			});
		}

		public string Conversion
		{
			get => GetValue(ConversionProperty);
			set => SetValue(ConversionProperty, value);
		}

		public decimal ConversionRate
		{
			get => GetValue(ConversionRateProperty);
			set => SetValue(ConversionRateProperty, value);
		}

		public string CurrencyCode
		{
			get => GetValue(CurrencyCodeProperty);
			set => SetValue(CurrencyCodeProperty, value);
		}

		public string ConversionCurrencyCode
		{
			get => GetValue(ConversionCurrencyCodeProperty);
			set => SetValue(ConversionCurrencyCodeProperty, value);
		}
	}
}
