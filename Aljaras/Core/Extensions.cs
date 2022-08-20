using System.Windows;

namespace Aljaras.Core
{
    public static class Extensions
    {
        public static readonly DependencyProperty CarrierProperty = DependencyProperty.RegisterAttached("Carrier", typeof(object), typeof(Extensions), new PropertyMetadata(default(string)));
        public static void SetCarrier(UIElement element, object value)
        {
            element.SetValue(CarrierProperty, value);
        }
        public static object GetCarrier(UIElement element)
        {
            return (object)element.GetValue(CarrierProperty);
        }

        public static readonly DependencyProperty Carrier2Property = DependencyProperty.RegisterAttached("Carrier2", typeof(object), typeof(Extensions), new PropertyMetadata(default(string)));
        public static void SetCarrier2(UIElement element, object value)
        {
            element.SetValue(Carrier2Property, value);
        }
        public static object GetCarrier2(UIElement element)
        {
            return (object)element.GetValue(Carrier2Property);
        }
    }
}
