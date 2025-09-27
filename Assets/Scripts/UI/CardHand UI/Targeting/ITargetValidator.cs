using System.Collections.Generic;

public interface ITargetValidator {
    bool CanValidateAllTargets(List<TypedTargetBase> targets);
}

public class TargetValidator : ITargetValidator {

    // Soon it will search and compose all possible targets
    public bool CanValidateAllTargets(List<TypedTargetBase> targets) {
        return true;
    }
}