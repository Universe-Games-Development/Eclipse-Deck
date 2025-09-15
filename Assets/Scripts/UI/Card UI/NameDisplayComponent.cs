// This is not Boiler plate! elements will have specific animations
public class NameDisplayComponent : SingleDisplayComponent {
    public override void UpdateDisplay(CardDisplayContext context) {
        text.text = context.Data.name;

        SetVisibility(context.Config.showName);
    }
}