
public class CategoryDisplayComponent : SingleDisplayComponent {
    public override void UpdateDisplay(CardDisplayContext context) {
        text.text = context.Data.cost.ToString();
        bool showCategory = context.Config.showCategory;
        SetVisibility(showCategory);
    }
}

