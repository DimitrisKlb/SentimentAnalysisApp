namespace WebSite.ViewModels {
    public class BannerMsg {

        public readonly string Text;
        public readonly string AlertType;

        protected BannerMsg(string Text, string AlertType) {
            this.Text = Text;
            this.AlertType = AlertType;
        }

    }
}