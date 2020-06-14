using System.Runtime.CompilerServices;

namespace Rystem.Web
{
    public class DefaultUI
    {
        public string Primary { get; set; }
        public string Secondary { get; set; }
        public string SelectedPrimary { get; set; }
        public string SelectedSecondary { get; set; }
        public string Success { get; set; }
        public string Info { get; set; }
        public string Warning { get; set; }
        public string Danger { get; set; }
        public string Light { get; set; }
        public string Dark { get; set; }
        public string White { get; set; }
        public static DefaultUI Default => new DefaultUI()
        {
            Primary = "#4E7374",
            Secondary = "#224A63",
            SelectedPrimary = "#3E6364",
            SelectedSecondary = "#123A53",
            Success = "#1cc88a",
            Info = "#36b9cc",
            Warning = "#f6c23e",
            Danger = "#e74a3b",
            Light = "#f8f9fc",
            Dark = "#5a5c69",
            White = "#fff"
        };
        public string ToCssVariables()
            => $"<style>:root {{--primary: {this.Primary};--secondary: {this.Secondary};--selectedprimary: {this.SelectedPrimary};--selectedsecondary: {this.SelectedSecondary};--success: {this.Success};--info: {this.Info};--warning: {this.Warning};--danger: {this.Danger};--light: {this.Light};--dark: {this.Dark};--white: {this.White};}}</style>";
    }
}