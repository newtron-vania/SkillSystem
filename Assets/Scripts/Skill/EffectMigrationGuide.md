# Effect System Migration Guide

## Overview

This guide covers the migration from the old Effect system to the new SkillResult-based Effect system with proper error handling and initialization patterns.

## Key Changes

### 1. Effect Base Class Changes

**Before:**
```csharp
public abstract class Effect
{
    public abstract void Initialize(params object[] objects);
    public abstract bool Apply(Actor source, Actor target);
    public abstract void Clear();
}
```

**After:**
```csharp
public abstract class Effect
{
    public abstract SkillResult Initialize(params object[] objects);
    public abstract bool Apply(Actor source, Actor target);
    public abstract void Clear();
    public virtual bool IsInitialized { get; protected set; } = false;

    // Helper methods for SkillResult creation
    protected SkillResult Success(string message = "") => SkillResult.Success(message);
    protected SkillResult Failure(string message) => SkillResult.Failure(message);
    protected SkillResult Failure(string message, Exception error) => SkillResult.Failure(message, error);
}
```

### 2. Skill Base Class Changes

**Before:**
```csharp
public class RocketGrabSkill : NoneTargetSkill
{
    public RocketGrabSkill(SkillData skillData) : base(skillData)
    {
        AddEffect(new ConsumeManaEffect(Cost));
        // Direct constructor parameter passing
    }

    public override void Initialize()
    {
        GetEffect(1).Initialize(Cost); // Direct access
    }

    public override bool ApplySkill(Actor source)
    {
        var effect = GetEffect(_currentEffectIndex); // Direct access
        return effect.Apply(source, target);
    }
}
```

**After:**
```csharp
public class RocketGrabSkill : NoneTargetSkill
{
    public RocketGrabSkill(SkillData skillData,
                          IInputService inputService = null,
                          ITriggerService triggerService = null,
                          ITargetingService targetingService = null,
                          ISkillManagerService skillManagerService = null)
        : base(skillData, inputService, triggerService, targetingService, skillManagerService)
    {
        // Create effects with default constructors
        AddEffect(new ConsumeManaEffect());
    }

    public override SkillResult Initialize()
    {
        // Safe access with SkillResult
        var effect0Result = GetEffect(0);
        if (effect0Result.IsSuccess)
        {
            var initResult = effect0Result.Value.Initialize(Cost);
            if (!initResult.IsSuccess)
                return initResult;
        }
        else
        {
            return SkillResult.Failure($"Failed to get effect: {effect0Result.Message}");
        }

        return base.Initialize();
    }

    public override SkillResult ApplySkill(Actor source)
    {
        var effectResult = GetEffect(_currentEffectIndex);
        if (!effectResult.IsSuccess)
        {
            return SkillResult.Failure($"Failed to get effect: {effectResult.Message}");
        }

        var success = effectResult.Value.Apply(source, target);
        return SkillResult.Success("Effect applied");
    }
}
```

## Migration Steps

### Step 1: Update Effect Implementations

For each Effect class, update the Initialize method:

**Before:**
```csharp
public class ConsumeManaEffect : Effect
{
    public override void Initialize(params object[] objects)
    {
        if (objects[0] is float cost) _cost = cost;
    }
}
```

**After:**
```csharp
public class ConsumeManaEffect : Effect
{
    public override SkillResult Initialize(params object[] objects)
    {
        try
        {
            if (objects.Length == 0)
                return Failure("ConsumeManaEffect.Initialize: No parameters provided");

            if (objects[0] is float cost)
            {
                _cost = cost;
                IsInitialized = true;
                return Success($"ConsumeManaEffect initialized with cost: {_cost}");
            }

            return Failure($"ConsumeManaEffect.Initialize: Expected float, got {objects[0]?.GetType()?.Name ?? "null"}");
        }
        catch (Exception ex)
        {
            return Failure("ConsumeManaEffect.Initialize failed", ex);
        }
    }
}
```

### Step 2: Update Skill Constructors

Change from constructor parameter injection to default constructors + Initialize pattern:

**Before:**
```csharp
AddEffect(new ConsumeManaEffect(Cost));
AddEffect(new CooldownEffect(Cooltime, SkillIndex));
```

**After:**
```csharp
AddEffect(new ConsumeManaEffect());
AddEffect(new CooldownEffect());
```

### Step 3: Update Skill Initialize Methods

**Before:**
```csharp
public override void Initialize()
{
    GetEffect(0).Initialize(Cost);
    GetEffect(1).Initialize(Cooltime, SkillIndex);
}
```

**After:**
```csharp
public override SkillResult Initialize()
{
    try
    {
        var results = new List<SkillResult>();

        // Effect 0: ConsumeManaEffect
        var effect0Result = GetEffect(0);
        if (effect0Result.IsSuccess)
        {
            var initResult = effect0Result.Value.Initialize(Cost);
            results.Add(initResult);
        }
        else
        {
            return SkillResult.Failure($"Failed to get ConsumeManaEffect: {effect0Result.Message}");
        }

        // Check all initialization results
        var failedResults = results.Where(r => !r.IsSuccess).ToList();
        if (failedResults.Any())
        {
            var errorMessages = string.Join(", ", failedResults.Select(r => r.Message));
            return SkillResult.Failure($"Effect initialization failed: {errorMessages}");
        }

        return base.Initialize();
    }
    catch (Exception ex)
    {
        return SkillResult.Failure("Skill initialization failed", ex);
    }
}
```

### Step 4: Update ApplySkill Methods

**Before:**
```csharp
public override bool ApplySkill(Actor source)
{
    var effect = GetEffect(_currentEffectIndex);
    return effect.Apply(source, target);
}
```

**After:**
```csharp
public override SkillResult ApplySkill(Actor source)
{
    var baseResult = base.ApplySkill(source);
    if (!baseResult.IsSuccess)
        return baseResult;

    var effectResult = GetEffect(_currentEffectIndex);
    if (!effectResult.IsSuccess)
    {
        return SkillResult.Failure($"Failed to get effect at index {_currentEffectIndex}: {effectResult.Message}");
    }

    var success = effectResult.Value.Apply(source, target);
    if (success)
    {
        _currentEffectIndex++;
    }

    return SkillResult.Success("Effect applied");
}
```

## Error Handling Patterns

### Using the Error Handling Helper

```csharp
// For simple initialization
var initResult = EffectErrorHandling.InitializeEffects(
    (effect1, new object[] { Cost }),
    (effect2, new object[] { Cooltime, SkillIndex })
);

// For type-safe parameter validation
if (EffectErrorHandling.ValidateParameters<float, int>(objects, out float cost, out int skillIndex).IsSuccess)
{
    _cost = cost;
    _skillIndex = skillIndex;
    IsInitialized = true;
    return Success($"Initialized with cost: {cost}, skillIndex: {skillIndex}");
}
```

### Using the Builder Pattern

```csharp
public override SkillResult Initialize()
{
    var builder = new EffectInitializationBuilder();

    var effect0Result = GetEffect(0);
    if (effect0Result.IsSuccess)
        builder.AddEffect(effect0Result.Value, Cost);

    var effect1Result = GetEffect(1);
    if (effect1Result.IsSuccess)
        builder.AddEffect(effect1Result.Value, Cooltime, SkillIndex);

    return builder.Initialize();
}
```

## Common Migration Issues

### Issue 1: Direct Effect Access
**Problem:** `GetEffect(index)` returns `SkillResult<Effect>`, not `Effect`
**Solution:** Always check `IsSuccess` before accessing `Value`

### Issue 2: Constructor Parameter Injection
**Problem:** Effects constructed with parameters in constructor
**Solution:** Use default constructors and Initialize with parameters

### Issue 3: Missing Error Handling
**Problem:** No validation of initialization results
**Solution:** Use SkillResult pattern and validate all operations

### Issue 4: Service Injection
**Problem:** Skills constructed without service parameters
**Solution:** Update constructors to accept optional service parameters

## Best Practices

1. **Always validate SkillResult before accessing Value**
2. **Use helper methods for type-safe parameter validation**
3. **Provide meaningful error messages**
4. **Set IsInitialized flag in Effect Initialize methods**
5. **Use builder pattern for complex initialization scenarios**
6. **Handle exceptions gracefully with try-catch blocks**
7. **Log initialization failures for debugging**

## Testing Migration

```csharp
// Test Effect initialization
var effect = new ConsumeManaEffect();
var result = effect.Initialize(50f);
Assert.IsTrue(result.IsSuccess);
Assert.IsTrue(effect.IsInitialized);

// Test Skill initialization
var skill = new RocketGrabSkill(skillData);
var initResult = skill.Initialize();
Assert.IsTrue(initResult.IsSuccess);

// Test error handling
var badResult = effect.Initialize("invalid");
Assert.IsFalse(badResult.IsSuccess);
Assert.IsTrue(badResult.Message.Contains("Expected float"));
```

## Performance Considerations

- SkillResult adds minimal overhead compared to direct access
- Validation checks prevent runtime errors that would be more expensive
- Proper initialization reduces debugging time significantly
- Error messages help with faster issue resolution

## Debugging Tips

1. Use `EffectErrorHandling.LogEffectState()` for debugging
2. Check `IsInitialized` flags before Apply calls
3. Examine SkillResult messages for detailed error information
4. Use Unity debugger to inspect SkillResult values
5. Add temporary Debug.Log statements in Initialize methods