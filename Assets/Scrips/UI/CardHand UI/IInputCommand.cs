using System.Collections.Generic;

public interface IInputCommand : ICommand {
    List<BoardInput> GetRequiredInputs();
}
