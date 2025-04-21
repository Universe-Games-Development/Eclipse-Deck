using Cysharp.Threading.Tasks;

// Clean presenter pattern implementation
public class FieldPresenter {
    public readonly Field Model;
    public readonly FieldView View;

    public FieldPresenter(Field field, FieldView view) {
        this.Model = field;
        this.View = view;

        HandleOwnerChanged(field.Owner);
        HandleTypeChanged(field.FieldType);

        // Subscribe to field events
        field.OwnerChanged += HandleOwnerChanged;
        field.TypeChanged += HandleTypeChanged;
        field.CreaturePlaced += HandleCreaturePlaced;
        field.CreatureRemoved += HandleCreatureRemoved;
        field.FieldRemoved += HandleFieldRemoved;
    }


    public void Cleanup() {
        // Unsubscribe from events
        Model.OwnerChanged -= HandleOwnerChanged;
        Model.TypeChanged -= HandleTypeChanged;
        Model.CreaturePlaced -= HandleCreaturePlaced;
        Model.CreatureRemoved -= HandleCreatureRemoved;
        Model.FieldRemoved -= HandleFieldRemoved;
    }

    // Event handlers
    private void HandleOwnerChanged(Opponent opponent) {
        View.UpdateOwnerVisuals(opponent);
    }

    private void HandleTypeChanged(FieldType type) {
        View.UpdateTypeVisuals(type);
    }

    private void HandleCreaturePlaced(Creature creature) {
        View.UpdateCreatureVisuals(creature);
    }

    private void HandleCreatureRemoved(Creature creature) {
        View.UpdateCreatureVisuals(null);
    }

    private void HandleFieldRemoved(Field field) {
        View.RemoveWithAnimation().Forget();
    }
}
