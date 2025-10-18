using System.Collections.Generic;
using System.Linq;
using Zenject;

public interface ITargetValidator {
    // Перевіряє чи всі цілі можуть бути заповнені
    bool CanValidateAllTargets(List<TargetInfo> targets, string ownerId);

    // Отримує всі валідні юніти для конкретної цілі
    List<UnitModel> GetValidTargetsFor(TargetInfo target, string ownerId);

    // Перевіряє чи конкретний юніт валідний для цілі
    ValidationResult ValidateTarget(TargetInfo target, UnitModel unit, string ownerId);
}

public class TargetValidator : ITargetValidator {
    [Inject] private IUnitRegistry _unitRegistry;
    [Inject] private ILogger _logger;

    public bool CanValidateAllTargets(List<TargetInfo> targets, string ownerId) {
        if (targets?.Any() != true) {
            return false;
        }

        foreach (var target in targets) {
            var validTargets = GetValidTargetsFor(target, ownerId);

            // Якщо це обов'язкова ціль і немає валідних варіантів
            if (validTargets.Count == 0) {
                _logger.LogWarning(
                    $"No valid targets found for {target.Key} (owner: {ownerId})");
                return false;
            }
        }

        return true;
    }

    public List<UnitModel> GetValidTargetsFor(TargetInfo target, string ownerId) {
        var validModels = new List<UnitModel>();
        var allModels = _unitRegistry.GetAllModels<UnitModel>();
        var context = new ValidationContext(ownerId);

        foreach (var model in allModels) {
            var validationResult = target.IsValid(model, context);
            if (validationResult.IsValid) {
                validModels.Add(model);
            }
        }

        return validModels;
    }

    public ValidationResult ValidateTarget(TargetInfo target, UnitModel unit, string ownerId) {
        var context = new ValidationContext(ownerId);
        return target.IsValid(unit, context);
    }
}