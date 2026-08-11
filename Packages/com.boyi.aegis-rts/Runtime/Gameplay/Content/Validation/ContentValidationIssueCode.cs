namespace AegisRTS.Gameplay.Content.Validation
{
    /// <summary>Machine-readable category for a content validation failure.</summary>
    public enum ContentValidationIssueCode
    {
        DuplicateDefinitionId,
        DuplicateTag,
        MissingReference,
        InvalidStat,
        InvalidCost,
        TechnologyCycle,
        MissingPrefab,
        MissingTag,
        MissingRuleSet,
        InvalidDisplayName,
        NullDefinition,
    }
}
