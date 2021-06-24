using JetBrains.Application.Settings;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.EditorConfig;
using JetBrains.ReSharper.Psi.Format;

namespace JetBrains.ReSharper.Plugins.FSharp.Services.Formatter
{
  [SettingsKey(typeof(CodeFormattingSettingsKey), "Code formatting in F#")]
  [EditorConfigKey("fsharp")]
  public class FSharpFormatSettingsKey : FormatSettingsKeyBase
  {
    [SettingsEntry(EmptyBlockStyle.TOGETHER_SAME_LINE, "todo")]
    public EmptyBlockStyle EmptyBlockStyle;

    // todo: [EditorConfigEntryAlias("brace_style", EditorConfigAliasType.ReSharperGeneralized)]
    [SettingsEntry(BraceFormatStyleEx.PICO, "Type and namespace declaration")]
    public BraceFormatStyleEx TypeDeclarationBraces;

    [SettingsEntry(2, "todo")]
    public int BlankLineAroundTopLevelModules;

    [SettingsEntry(0, "todo")]
    public int BlankLinesAroundSingleLineModuleMember;

    [SettingsEntry(1, "todo")]
    public int BlankLinesAroundMultilineModuleMembers;

    [SettingsEntry(1, "todo")]
    public int BlankLinesAroundDifferentModuleMemberKinds;

    [SettingsEntry(2, "todo")]
    public int KeepMaxBlankLineAroundModuleMembers;

    [SettingsEntry(1, "todo")]
    public int BlankLinesBeforeFirstModuleMemberInTopLevelModule;

    [SettingsEntry(0, "todo")]
    public int BlankLinesBeforeFirstModuleMemberInNestedModule;

    [SettingsEntry(true, "Line break after type representation access modifier")]
    public bool LineBreakAfterTypeReprAccessModifier;

    [SettingsEntry(true, "Keep line break after '=' in declarations")]
    public bool KeepExistingLineBreakBeforeDeclarationBody;

    [SettingsEntry(PlaceOnSameLineAsOwner.IF_OWNER_IS_SINGLE_LINE, "Place declaration body on the same line")]
    public PlaceOnSameLineAsOwner DeclarationBodyOnTheSameLine;

    [SettingsEntry(false, "Semicolon at end of line")]
    [EditorConfigEntryAlias("semicolon_at_end_of_line", EditorConfigAliasType.LanguageSpecific)]
    public bool SemicolonAtEndOfLine;

    [SettingsEntry(true, "Space before parameter")]
    [EditorConfigEntryAlias("space_before_parameter", EditorConfigAliasType.LanguageSpecific)]
    public bool SpaceBeforeParameter;

    [SettingsEntry(true, "Space before lowercase function invocation")]
    [EditorConfigEntryAlias("space_before_lowercase_invocation", EditorConfigAliasType.LanguageSpecific)]
    public bool SpaceBeforeLowercaseInvocation;

    [SettingsEntry(false, "Space before uppercase function invocation")]
    [EditorConfigEntryAlias("space_before_uppercase_invocation", EditorConfigAliasType.LanguageSpecific)]
    public bool SpaceBeforeUppercaseInvocation;

    [SettingsEntry(false, "Space before class constructor")]
    [EditorConfigEntryAlias("space_before_class_constructor", EditorConfigAliasType.LanguageSpecific)]
    public bool SpaceBeforeClassConstructor;

    [SettingsEntry(false, "Space before member")]
    [EditorConfigEntryAlias("space_before_member", EditorConfigAliasType.LanguageSpecific)]
    public bool SpaceBeforeMember;

    [SettingsEntry(false, "Space before colon")]
    [EditorConfigEntryAlias("space_before_colon", EditorConfigAliasType.LanguageSpecific)]
    public bool SpaceBeforeColon;

    [SettingsEntry(true, "Space after comma")]
    [EditorConfigEntryAlias("space_after_comma", EditorConfigAliasType.LanguageSpecific)]
    public bool SpaceAfterComma;

    [SettingsEntry(false, "Space before semicolon")]
    [EditorConfigEntryAlias("space_before_semicolon", EditorConfigAliasType.LanguageSpecific)]
    public bool SpaceBeforeSemicolon;

    [SettingsEntry(true, "Space after semicolon")]
    [EditorConfigEntryAlias("space_after_semicolon", EditorConfigAliasType.LanguageSpecific)]
    public bool SpaceAfterSemicolon;

    [SettingsEntry(false, "Indent on try with")]
    [EditorConfigEntryAlias("indent_on_try_with", EditorConfigAliasType.LanguageSpecific)]
    public bool IndentOnTryWith;

    [SettingsEntry(true, "Space around delimiter")]
    [EditorConfigEntryAlias("space_around_delimiter", EditorConfigAliasType.LanguageSpecific)]
    public bool SpaceAroundDelimiter;

    [SettingsEntry(40, "Maximum width of longer (non multiline) if/then/else expressions")]
    [EditorConfigEntryAlias("max_if_then_else_short_width", EditorConfigAliasType.LanguageSpecific)]
    public int MaxIfThenElseShortWidth;

    [SettingsEntry(50, "Maximum width of single line infix expressions")]
    [EditorConfigEntryAlias("max_infix_operator_expression", EditorConfigAliasType.LanguageSpecific)]
    public int MaxInfixOperatorExpression;

    [SettingsEntry(40, "Maximum width of single line record expressions")]
    [EditorConfigEntryAlias("max_record_width", EditorConfigAliasType.LanguageSpecific)]
    public int MaxRecordWidth;

    [SettingsEntry(40, "Maximum width of single line array expressions")]
    [EditorConfigEntryAlias("max_array_or_list_width", EditorConfigAliasType.LanguageSpecific)]
    public int MaxArrayOrListWidth;

    [SettingsEntry(40, "Maximum width of value binding expressions")]
    [EditorConfigEntryAlias("max_value_binding_width", EditorConfigAliasType.LanguageSpecific)]
    public int MaxValueBindingWidth;

    [SettingsEntry(40, "Maximum width of function binding expressions")]
    [EditorConfigEntryAlias("max_function_binding_width", EditorConfigAliasType.LanguageSpecific)]
    public int MaxFunctionBindingWidth;

    [SettingsEntry(false, "Align opening and closing braces of record, array and list expressions")]
    [EditorConfigEntryAlias("multiline_block_brackets_on_same_column", EditorConfigAliasType.LanguageSpecific)]
    public bool MultilineBlockBracketsOnSameColumn;

    [SettingsEntry(false, "Newline between type definition and members")]
    [EditorConfigEntryAlias("newline_between_type_definition_and_members", EditorConfigAliasType.LanguageSpecific)]
    public bool NewlineBetweenTypeDefinitionAndMembers;

    [SettingsEntry(false, "Keep if and then keywords in same line")]
    [EditorConfigEntryAlias("keep_if_then_in_same_line", EditorConfigAliasType.LanguageSpecific)]
    public bool KeepIfThenInSameLine;

    [SettingsEntry(40, "Maximum width of Elmish inspired expressions")]
    [EditorConfigEntryAlias("max_elmish_width", EditorConfigAliasType.LanguageSpecific)]
    public int MaxElmishWidth;

    [SettingsEntry(false, "Single argument Elmish expressions")]
    [EditorConfigEntryAlias("single_argument_web_mode", EditorConfigAliasType.LanguageSpecific)]
    public bool SingleArgumentWebMode;

    [SettingsEntry(false, "Align function signature to indentation")]
    [EditorConfigEntryAlias("align_function_signature_to_indentation", EditorConfigAliasType.LanguageSpecific)]
    public bool AlignFunctionSignatureToIndentation;

    [SettingsEntry(false, "Alternative long member definitions")]
    [EditorConfigEntryAlias("alternative_long_member_definitions", EditorConfigAliasType.LanguageSpecific)]
    public bool AlternativeLongMemberDefinitions;

    [SettingsEntry(true, "Don't indent comments started at first column")]
    public bool StickComment;

    [SettingsEntry(true, "Don't report parens in application expressions")]
    public bool AllowHighPrecedenceAppParens;

    [SettingsEntry(true, "Outdent binary operators")]
    public bool OutdentBinaryOperators;

    [SettingsEntry(true, "Never outdent pipe operators")]
    public bool NeverOutdentPipeOperators;

    [SettingsIndexedEntry("List of Fantomas settings")]
    public readonly IIndexedEntry<string, string> FantomasSettings;
  }
}
