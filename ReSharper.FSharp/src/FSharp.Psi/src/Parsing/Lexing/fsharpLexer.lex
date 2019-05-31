using System.Collections;
using JetBrains.Util;
using JetBrains.Text;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing;
using static JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing.FSharpTokenType;

%%

%unicode

%init{
   currTokenType = null;
%init}

%{

%}

%eofval{
  if(yy_lexical_state == IN_BLOCK_COMMENT || yy_lexical_state == IN_BLOCK_COMMENT_FROM_LINE)
  {
    return fillBlockComment(UNFINISHED_BLOCK_COMMENT);
  }
  else
    return makeToken(null);
%eofval}

%namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing.Lexing
%class FSharpLexerGenerated
%public
%implements IIncrementalLexer
%function _locateToken
%virtual
%type TokenNodeType

%state IN_BLOCK_COMMENT
%state IN_BLOCK_COMMENT_FROM_LINE
%state STRING_IN_COMMENT
%state SMASH_INT_DOT_DOT
%state SMASH_INT_DOT_DOT_FROM_LINE
%state SMASH_RQUOTE_DOT
%state SMASH_RQUOTE_DOT_FROM_LINE
%state SMASH_ADJACENT_LESS_OP
%state SMASH_ADJACENT_GREATER_BAR_RBRACK
%state SMASH_ADJACENT_GREATER_RBRACK
%state SMASH_ADJACENT_GREATER_BAR_RBRACK_FIN
%state SMASH_ADJACENT_GREATER_RBRACK_FIN
%state ADJACENT_TYPE_CLOSE_OP
%state INIT_ADJACENT_TYAPP
%state ADJACENT_TYAPP
%state SYMBOLIC_OPERATOR
%state GREATER_OP
%state GREATER_OP_SYMBOLIC_OP
%state PRE_LESS_OP
%state LINE

%state PPSHARP
%state BAD_PPSHARP
%state PPSYMBOL
%state PPDIRECTIVE

WHITESPACE=((" ")+)
TAB=((\t)+)
ANYWHITE=({WHITESPACE}|{TAB})

%include PsiTasks/Unicode.lex

NEW_LINE=(\n|\r\n)
LINE_COMMENT=(\/\/([^\n\r])*)
SHEBANG=("#!"([^\n\r])*)
LETTER_CHAR=({UNICODE_LU}|{UNICODE_LL}|{UNICODE_LT}|{UNICODE_LM}|{UNICODE_LO}|{UNICODE_NL})
DIGIT=({UNICODE_ND})
IDENT_START_CHAR=({LETTER_CHAR}|"_")
CONNECTING_CHAR=({UNICODE_PC})
COMBINING_CHAR=({UNICODE_MN}|{UNICODE_MC})
FORMATTING_CHAR=({UNICODE_CF})
IDENT_CHAR=({LETTER_CHAR}|{CONNECTING_CHAR}|{COMBINING_CHAR}|{FORMATTING_CHAR}|{DIGIT}|['_])
TAIL_IDENT=(({IDENT_CHAR})+)
IDENT_TEXT=(({IDENT_START_CHAR})({TAIL_IDENT})?)
IDENT=({IDENT_TEXT}|("``"(([^`\n\r\t]|("`"[^`\n\r\t]))+)"``"))
RESERVED_IDENT_FORMATS=({IDENT_TEXT}([!#]))
ESCAPE_CHAR=[\\][\\\"\'afvntbr]
NON_ESCAPE_CHARS=[\\][^afvntbr\\\"\']
SIMPLE_CHARACTER=[^\t\b\n\r\\\'\"]
SIMPLE_STRING_CHAR=[^\"\\]
HEXGRAPH_SHORT=\\x{HEXDIGIT}{HEXDIGIT}
UNICODEGRAPH_SHORT=\\u{HEXDIGIT}{HEXDIGIT}{HEXDIGIT}{HEXDIGIT}
UNICODEGRAPH_LONG=\\U{HEXDIGIT}{HEXDIGIT}{HEXDIGIT}{HEXDIGIT}{HEXDIGIT}{HEXDIGIT}{HEXDIGIT}{HEXDIGIT}
TRIGRAPH=\\{DIGIT}{DIGIT}{DIGIT}
CHARACTER=({SIMPLE_CHARACTER}|{ESCAPE_CHAR}|{TRIGRAPH}|{UNICODEGRAPH_SHORT}|{HEXGRAPH_SHORT}|{UNICODEGRAPH_LONG}|\")
STRING_CHAR=({SIMPLE_STRING_CHAR}|{ESCAPE_CHAR}|{NON_ESCAPE_CHARS}|{TRIGRAPH}|{UNICODEGRAPH_SHORT}|{UNICODEGRAPH_LONG}|\')
CHARACTER_LITERAL=(\'{CHARACTER}\')
UNFINISHED_STRING=(\"({STRING_CHAR})*)
STRING=({UNFINISHED_STRING}\")
VERBATIM_STRING_CHAR=({SIMPLE_STRING_CHAR}|{NON_ESCAPE_CHARS}|\"\"|\\)
UNFINISHED_VERBATIM_STRING=(@\"({VERBATIM_STRING_CHAR})*)
VERBATIM_STRING=({UNFINISHED_VERBATIM_STRING}\")
BYTECHAR=(\'({SIMPLE_OR_ESCAPE_CHAR}|{TRIGRAPH}|{UNICODEGRAPH_SHORT})\'B)
BYTEARRAY=(\"({STRING_CHAR})*\"B)
VERBATIM_BYTEARRAY=(@\"({VERBATIM_STRING_CHAR})*\"B)
SIMPLE_OR_ESCAPE_CHAR=({ESCAPE_CHAR}|{SIMPLE_CHARACTER})
TRIPLE_QUOTED_STRING_CHAR=({SIMPLE_STRING_CHAR}|{NON_ESCAPE_CHARS}|{NEW_LINE}|\\)
UNFINISHED_TRIPLE_QUOTED_STRING=(\"\"\"((\"?){TRIPLE_QUOTED_STRING_CHAR}|\"\"{TRIPLE_QUOTED_STRING_CHAR})*(\"|\"\")?)
TRIPLE_QUOTED_STRING=({UNFINISHED_TRIPLE_QUOTED_STRING}\"\"\")

LET_BANG="let!"
USE_BANG="use!"
DO_BANG="do!"
YIELD_BANG="yield!"
RETURN_BANG="return!"
MATCH_BANG="match!"
BAR="|"
RARROW="->"
LARROW="<-"
DOT="."
COLON=":"
LPAREN="("
RPAREN=")"
STAR="*"
LBRACK="["
RBRACK="]"
LESS="<"
GREATER=">"
LBRACK_LESS="[<"
GREATER_RBRACK=">]"
GREATER_BAR_RBRACK=">|]"
LBRACK_BAR="[|"
LBRACE_BAR="{|"
BAR_RBRACK="|]"
BAR_RBRACE="|}"
LBRACE="{"
RBRACE="}"
QUOTE="'"
HASH="#"
COLON_QMARK_GREATER=":?>"
COLON_QMARK=":?"
COLON_GREATER=":>"
DOT_DOT=\.\.
COLON_COLON="::"
COLON_EQUALS=":="
SEMICOLON_SEMICOLON=";;"
SEMICOLON=";"
EQUALS="="
UNDERSCORE="_"
QMARK="?"
QMARK_QMARK="??"
LPAREN_STAR_RPAREN="(*)"
MINUS="-"
PLUS="+"
DOLLAR="$"
PERCENT="%"
PERCENT_PERCENT="%%"
AMP="&"
AMP_AMP="&&"
COMMA=","

RESERVED_SYMBOLIC_SEQUENCE=[~`]

OP_CHAR=([!%&*+\-./<=>@^|~\?])
BAD_OP_CHAR=({OP_CHAR}|([$:]))
IGNORED_OP_CHAR=([.\?])
BAD_IGNORED_OP_CHAR=([.$\?])
LQUOTE_TYPED="<@"
RQUOTE_TYPED="@>"
LQUOTE_UNTYPED="<@@"
RQUOTE_UNTYPED="@@>"
QUOTE_OP_RIGHT=({RQUOTE_TYPED}|{RQUOTE_UNTYPED})
QUOTE_OP_LEFT=({LQUOTE_TYPED}|{LQUOTE_UNTYPED})
SYMBOLIC_OP=(([\?])|("?<-")|(({OP_CHAR})+)|{QUOTE_OP_LEFT}|{QUOTE_OP_RIGHT})
BAD_SYMBOLIC_OP=(([\?])|("?<-")|(({BAD_OP_CHAR})+)|{QUOTE_OP_LEFT}|{QUOTE_OP_RIGHT})

INFIX_STAR_STAR_OP=(({IGNORED_OP_CHAR})*("**")({OP_CHAR})*)
BAD_INFIX_STAR_STAR_OP=(({BAD_IGNORED_OP_CHAR})*("**")({BAD_OP_CHAR})*)
INFIX_STAR_DIV_MOD_OP=(({IGNORED_OP_CHAR})*([*/%])({OP_CHAR})*)
BAD_INFIX_STAR_DIV_MOD_OP=(({BAD_IGNORED_OP_CHAR})*([*/%])({BAD_OP_CHAR})*)
PLUS_MINUS_OP=(({IGNORED_OP_CHAR})*([+\-])({OP_CHAR})*)
BAD_PLUS_MINUS_OP=(({BAD_IGNORED_OP_CHAR})*([+\-])({BAD_OP_CHAR})*)
INFIX_AT_HAT_OP=(({IGNORED_OP_CHAR})*([@^])({OP_CHAR})*)
BAD_INFIX_AT_HAT_OP=(({BAD_IGNORED_OP_CHAR})*([@^])({BAD_OP_CHAR})*)
INFIX_COMPARE_OP=(({IGNORED_OP_CHAR})*(([=<>]|("!=")))({OP_CHAR})*)
BAD_INFIX_COMPARE_OP=(({BAD_IGNORED_OP_CHAR})*(([=<>]|("!=")))({BAD_OP_CHAR})*)
INFIX_AMP_OP=(({IGNORED_OP_CHAR})*("&")({OP_CHAR})*)
BAD_INFIX_AMP_OP=(({BAD_IGNORED_OP_CHAR})*("&")({BAD_OP_CHAR})*)
INFIX_BAR_OP=(({IGNORED_OP_CHAR})*("|")({OP_CHAR})*)
BAD_INFIX_BAR_OP=(({BAD_IGNORED_OP_CHAR})*("|")({BAD_OP_CHAR})*)
PREFIX_OP=(({IGNORED_OP_CHAR})*("|")({OP_CHAR})*)
BAD_PREFIX_OP=(({BAD_IGNORED_OP_CHAR})*([!~])({BAD_OP_CHAR})*)
FUNKY_OPERATOR_NAME=(".[]"|".[]<-"|".[,]<-"|".[,,]<-"|".[,,,]<-"|".[,,,]"|".[,,]"|".[,]"|".[..]"|".[..,..]"|".[..,..,..]"|".[..,..,..,..]"|".()"|".()<-")

LESS_OP=(("</")|{LESS})

SEPARATOR="_"
HEXDIGIT=({DIGIT}|[A-F]|[a-f])
OCTALDIGIT=[0-7]
BITDIGIT=[0-1]
INT=({DIGIT}(({DIGIT}|{SEPARATOR})*{DIGIT})?)
XINT=(0(((x|X){HEXDIGIT}+)|((o|O){OCTALDIGIT}+)|((b|B){BITDIGIT}+)))
SBYTE=(({INT}|{XINT})y)
BYTE=(({INT}|{XINT})uy)
INT16=(({INT}|{XINT})s)
UINT16=(({INT}|{XINT})us)
INT32=(({INT}|{XINT})l)
UINT32=(({INT}|{XINT})u(l)?)
NATIVEINT=(({INT}|{XINT})n)
UNATIVEINT=(({INT}|{XINT})un)
INT64=(({INT}|{XINT})L)
UINT64=(({INT}|{XINT})[Uu]L)
IEEE32=({FLOAT}[Ff]|{XINT}lf)
IEEE64=({FLOAT}|{XINT}LF)
BIGNUM=({INT}[QRZING])
DECIMAL=(({FLOAT}|{INT})[Mm])
FLOATP=({DIGIT}(({DIGIT}|{SEPARATOR})*{DIGIT})?\.({DIGIT}(({DIGIT}|{SEPARATOR})*{DIGIT})?)?)
FLOATE=({DIGIT}(({DIGIT}|{SEPARATOR})*{DIGIT})?(\.({DIGIT}(({DIGIT}|{SEPARATOR})*{DIGIT})?)?)?[eE][+\-]?{DIGIT}(({DIGIT}|{SEPARATOR})*{DIGIT})?)
FLOAT=({FLOATP}|{FLOATE})
RESERVED_LITERAL_FORMATS=(({INT}|{XINT}|{FLOAT})({TAIL_IDENT}))

KEYWORD_STRING_SOURCE_DIRECTORY="__SOURCE_DIRECTORY__"
KEYWORD_STRING_SOURCE_FILE="__SOURCE_FILE__"
KEYWORD_STRING_LINE="__LINE__"

PP_COMPILER_DIRECTIVE=({HASH}("if"|"else"|"endif"))
PP_BAD_COMPILER_DIRECTIVE=({HASH}("if"|"else"|"endif"){TAIL_IDENT})
PP_DIRECTIVE=(({ANYWHITE})*({HASH}({IDENT}|({ANYWHITE})*([0-9])+)))
PP_CONDITIONAL_SYMBOL={IDENT}

%%
<YYINITIAL> {PP_DIRECTIVE} { yypushback(yylength()); yybegin(PPDIRECTIVE); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<YYINITIAL> [^]            { yypushback(1); yybegin(LINE); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> {LESS}                        { deepIntoParenLevel(); return makeToken(LESS); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> {LPAREN}                      { deepIntoParenLevel(); return makeToken(LPAREN); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> {LBRACK}                      { deepIntoParenLevel(); return makeToken(LBRACK); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> {LBRACK_LESS}                 { deepIntoBrackLevel(); return makeToken(LBRACK_LESS); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> ({GREATER})+                  { riseFromParenLevel(yylength()); yypushback(yylength()); yybegin(GREATER_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "</"                          { deepIntoParenLevel(); yypushback(2); yybegin(SMASH_ADJACENT_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break;}
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> {RPAREN}                      { riseFromParenLevel(1); return makeToken(RPAREN); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> {RBRACK}                      { riseFromParenLevel(1); return makeToken(RBRACK); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> {GREATER_RBRACK}              { yypushback(yylength()); checkGreatRBrack(SMASH_ADJACENT_GREATER_RBRACK, SMASH_ADJACENT_GREATER_RBRACK_FIN); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> {GREATER_BAR_RBRACK}          { yypushback(yylength()); initSmashAdjacent(SMASH_ADJACENT_GREATER_BAR_RBRACK, SMASH_ADJACENT_GREATER_BAR_RBRACK_FIN); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> ({GREATER})+{BAD_SYMBOLIC_OP} { yypushback(yylength()); yybegin(ADJACENT_TYPE_CLOSE_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "default"  { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "struct"   { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "null"     { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "delegate" { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "and"      { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "when"     { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "new"      { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "global"   { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "const"    { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "true"     { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "false"    { return initIdent(); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "^"        { return makeToken(INFIX_AT_HAT_OP); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "^-"       { return makeToken(INFIX_AT_HAT_OP); }
<INIT_ADJACENT_TYAPP, ADJACENT_TYAPP> "/"        { return makeToken(INFIX_STAR_DIV_MOD_OP); }

<INIT_ADJACENT_TYAPP> {IDENT} { return identInInitTypeApp(); }
<ADJACENT_TYAPP> {IDENT}      { return identInTypeApp(); }

<ADJACENT_TYPE_CLOSE_OP> ({GREATER})+ { adjacentTypeCloseOp(); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<GREATER_OP, GREATER_OP_SYMBOLIC_OP> {GREATER} { return makeToken(GREATER); }
<GREATER_OP, GREATER_OP_SYMBOLIC_OP> [^]       { yypushback(1); exitGreaterOp(); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<SYMBOLIC_OPERATOR> {LQUOTE_TYPED}        { riseFromParenLevel(0); return makeToken(LQUOTE_TYPED); }
<SYMBOLIC_OPERATOR> {RQUOTE_TYPED}        { riseFromParenLevel(0); return makeToken(RQUOTE_TYPED); }
<SYMBOLIC_OPERATOR> {LQUOTE_UNTYPED}      { riseFromParenLevel(0); return makeToken(LQUOTE_UNTYPED); }
<SYMBOLIC_OPERATOR> {RQUOTE_UNTYPED}      { riseFromParenLevel(0); return makeToken(RQUOTE_UNTYPED); }
<SYMBOLIC_OPERATOR> "@>."|"@@>."          { yypushback(yylength()); yybegin(SMASH_RQUOTE_DOT); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<SYMBOLIC_OPERATOR> {BAR}                 { riseFromParenLevel(0); return makeToken(BAR); }
<SYMBOLIC_OPERATOR> {LARROW}              { riseFromParenLevel(0); return makeToken(LARROW); }
<SYMBOLIC_OPERATOR> {LPAREN}              { riseFromParenLevel(0); return makeToken(LPAREN); }
<SYMBOLIC_OPERATOR> {RPAREN}              { riseFromParenLevel(0); return makeToken(RPAREN); }
<SYMBOLIC_OPERATOR> {LBRACK}              { riseFromParenLevel(0); return makeToken(LBRACK); }
<SYMBOLIC_OPERATOR> {RBRACK}              { riseFromParenLevel(0); return makeToken(RBRACK); }
<SYMBOLIC_OPERATOR> {LBRACK_LESS}         { riseFromParenLevel(0); return makeToken(LBRACK_LESS); }
<SYMBOLIC_OPERATOR> {GREATER_RBRACK}      { riseFromParenLevel(0); return makeToken(GREATER_RBRACK); }
<SYMBOLIC_OPERATOR> {LBRACK_BAR}          { riseFromParenLevel(0); return makeToken(LBRACK_BAR); }
<SYMBOLIC_OPERATOR> {LBRACE_BAR}          { riseFromParenLevel(0); return makeToken(LBRACE_BAR); }
<SYMBOLIC_OPERATOR> {LESS}                { riseFromParenLevel(0); return makeToken(LESS); }
<SYMBOLIC_OPERATOR> {GREATER}             { riseFromParenLevel(0); return makeToken(GREATER); }
<SYMBOLIC_OPERATOR> {BAR_RBRACK}          { riseFromParenLevel(0); return makeToken(BAR_RBRACK); }
<SYMBOLIC_OPERATOR> {BAR_RBRACE}          { riseFromParenLevel(0); return makeToken(BAR_RBRACE); }
<SYMBOLIC_OPERATOR> {LBRACE}              { riseFromParenLevel(0); return makeToken(LBRACE); }
<SYMBOLIC_OPERATOR> {RBRACE}              { riseFromParenLevel(0); return makeToken(RBRACE); }
<SYMBOLIC_OPERATOR> {GREATER_BAR_RBRACK}  { riseFromParenLevel(0); return makeToken(GREATER_BAR_RBRACK); }
<SYMBOLIC_OPERATOR> {COLON_QMARK_GREATER} { riseFromParenLevel(0); return makeToken(COLON_QMARK_GREATER); }
<SYMBOLIC_OPERATOR> {COLON_QMARK}         { riseFromParenLevel(0); return makeToken(COLON_QMARK); }
<SYMBOLIC_OPERATOR> {COLON_COLON}         { riseFromParenLevel(0); return makeToken(COLON_COLON); }
<SYMBOLIC_OPERATOR> {COLON_EQUALS}        { riseFromParenLevel(0); return makeToken(COLON_EQUALS); }
<SYMBOLIC_OPERATOR> {SEMICOLON_SEMICOLON} { riseFromParenLevel(0); return makeToken(SEMICOLON_SEMICOLON); }
<SYMBOLIC_OPERATOR> {SEMICOLON}           { riseFromParenLevel(0); return makeToken(SEMICOLON); }
<SYMBOLIC_OPERATOR> {QMARK}               { riseFromParenLevel(0); return makeToken(QMARK); }
<SYMBOLIC_OPERATOR> {QMARK_QMARK}         { riseFromParenLevel(0); return makeToken(QMARK_QMARK); }
<SYMBOLIC_OPERATOR> {LPAREN_STAR_RPAREN}  { riseFromParenLevel(0); return makeToken(LPAREN_STAR_RPAREN); }
<SYMBOLIC_OPERATOR> {PLUS}                { riseFromParenLevel(0); return makeToken(PLUS); }
<SYMBOLIC_OPERATOR> {DOLLAR}              { riseFromParenLevel(0); return makeToken(DOLLAR); }
<SYMBOLIC_OPERATOR> {PERCENT}             { riseFromParenLevel(0); return makeToken(PERCENT); }
<SYMBOLIC_OPERATOR> {PERCENT_PERCENT}     { riseFromParenLevel(0); return makeToken(PERCENT_PERCENT); }
<SYMBOLIC_OPERATOR> {AMP}                 { riseFromParenLevel(0); return makeToken(AMP); }
<SYMBOLIC_OPERATOR> {AMP_AMP}             { riseFromParenLevel(0); return makeToken(AMP_AMP); }
<SYMBOLIC_OPERATOR> {RARROW}              { riseFromParenLevel(0); return makeToken(RARROW); }
<SYMBOLIC_OPERATOR> {DOT}                 { riseFromParenLevel(0); return makeToken(DOT); }
<SYMBOLIC_OPERATOR> {COLON}               { riseFromParenLevel(0); return makeToken(COLON); }
<SYMBOLIC_OPERATOR> {STAR}                { riseFromParenLevel(0); return makeToken(STAR); }
<SYMBOLIC_OPERATOR> {QUOTE}               { riseFromParenLevel(0); return makeToken(QUOTE); }
<SYMBOLIC_OPERATOR> {COLON_GREATER}       { riseFromParenLevel(0); return makeToken(COLON_GREATER); }
<SYMBOLIC_OPERATOR> {DOT_DOT}             { riseFromParenLevel(0); return makeToken(DOT_DOT); }
<SYMBOLIC_OPERATOR> {EQUALS}              { riseFromParenLevel(0); return makeToken(EQUALS); }
<SYMBOLIC_OPERATOR> {UNDERSCORE}          { riseFromParenLevel(0); return makeToken(UNDERSCORE); }
<SYMBOLIC_OPERATOR> {MINUS}               { riseFromParenLevel(0); return makeToken(MINUS); }
<SYMBOLIC_OPERATOR> {COMMA}               { riseFromParenLevel(0); return makeToken(COMMA); }

<SYMBOLIC_OPERATOR> {INFIX_STAR_STAR_OP}    { riseFromParenLevel(0); return makeToken(INFIX_STAR_STAR_OP); }
<SYMBOLIC_OPERATOR> {INFIX_STAR_DIV_MOD_OP} { riseFromParenLevel(0); return makeToken(INFIX_STAR_DIV_MOD_OP); }
<SYMBOLIC_OPERATOR> {PLUS_MINUS_OP}         { riseFromParenLevel(0); return makeToken(PLUS_MINUS_OP); }
<SYMBOLIC_OPERATOR> {INFIX_AT_HAT_OP}       { riseFromParenLevel(0); return makeToken(INFIX_AT_HAT_OP); }
<SYMBOLIC_OPERATOR> {INFIX_COMPARE_OP}      { riseFromParenLevel(0); return makeToken(INFIX_COMPARE_OP); }
<SYMBOLIC_OPERATOR> {INFIX_AMP_OP}          { riseFromParenLevel(0); return makeToken(INFIX_AMP_OP); }
<SYMBOLIC_OPERATOR> {INFIX_BAR_OP}          { riseFromParenLevel(0); return makeToken(INFIX_BAR_OP); }
<SYMBOLIC_OPERATOR> {PREFIX_OP}             { riseFromParenLevel(0); return makeToken(PREFIX_OP); }
<SYMBOLIC_OPERATOR> {FUNKY_OPERATOR_NAME}   { riseFromParenLevel(0); return makeToken(PREFIX_OP); }
<SYMBOLIC_OPERATOR> {SYMBOLIC_OP}           { riseFromParenLevel(0); return makeToken(SYMBOLIC_OP); }

<SYMBOLIC_OPERATOR> {BAD_INFIX_STAR_STAR_OP}    { riseFromParenLevel(0); return makeToken(BAD_INFIX_STAR_STAR_OP); }
<SYMBOLIC_OPERATOR> {BAD_INFIX_STAR_DIV_MOD_OP} { riseFromParenLevel(0); return makeToken(BAD_INFIX_STAR_DIV_MOD_OP); }
<SYMBOLIC_OPERATOR> {BAD_PLUS_MINUS_OP}         { riseFromParenLevel(0); return makeToken(BAD_PLUS_MINUS_OP); }
<SYMBOLIC_OPERATOR> {BAD_INFIX_AT_HAT_OP}       { riseFromParenLevel(0); return makeToken(BAD_INFIX_AT_HAT_OP); }
<SYMBOLIC_OPERATOR> {BAD_INFIX_COMPARE_OP}      { riseFromParenLevel(0); return makeToken(BAD_INFIX_COMPARE_OP); }
<SYMBOLIC_OPERATOR> {BAD_INFIX_AMP_OP}          { riseFromParenLevel(0); return makeToken(BAD_INFIX_AMP_OP); }
<SYMBOLIC_OPERATOR> {BAD_INFIX_BAR_OP}          { riseFromParenLevel(0); return makeToken(BAD_INFIX_BAR_OP); }
<SYMBOLIC_OPERATOR> {BAD_PREFIX_OP}             { riseFromParenLevel(0); return makeToken(BAD_PREFIX_OP); }
<SYMBOLIC_OPERATOR> {BAD_SYMBOLIC_OP}           { riseFromParenLevel(0); return makeToken(BAD_SYMBOLIC_OP); }

<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {TAB}        { return makeToken(BAD_TAB); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {WHITESPACE} { return makeToken(WHITESPACE); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {NEW_LINE}   { yybegin(YYINITIAL); return makeToken(NEW_LINE); }

<LINE, ADJACENT_TYAPP> "(*IF-FSHARP"    { return makeToken(BLOCK_COMMENT); }
<LINE, ADJACENT_TYAPP> "ENDIF-FSHARP*)" { return makeToken(BLOCK_COMMENT); }
<LINE, ADJACENT_TYAPP> "(*F#"           { return makeToken(BLOCK_COMMENT); }
<LINE, ADJACENT_TYAPP> "F#*)"           { return makeToken(BLOCK_COMMENT); }
<LINE, ADJACENT_TYAPP> {LINE_COMMENT}   { return makeToken(LINE_COMMENT); }
<LINE, ADJACENT_TYAPP> {SHEBANG}        { return makeToken(SHEBANG); }
<LINE, ADJACENT_TYAPP> {HASH}"light"    { return makeToken(PP_LIGHT); }

<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> ("(*")                  { initBlockComment(); initTokenLength(); increaseTokenLength(yylength()); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {PP_COMPILER_DIRECTIVE} { yypushback(yylength()); yybegin(BAD_PPSHARP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<INIT_ADJACENT_TYAPP> "(*IF-FSHARP"    { yybegin(LINE); return makeToken(BLOCK_COMMENT); }
<INIT_ADJACENT_TYAPP> "ENDIF-FSHARP*)" { yybegin(LINE); return makeToken(BLOCK_COMMENT); }
<INIT_ADJACENT_TYAPP> "(*F#"           { yybegin(LINE); return makeToken(BLOCK_COMMENT); }
<INIT_ADJACENT_TYAPP> "F#*)"           { yybegin(LINE); return makeToken(BLOCK_COMMENT); }
<INIT_ADJACENT_TYAPP> {LINE_COMMENT}   { yybegin(LINE); return makeToken(LINE_COMMENT); }
<INIT_ADJACENT_TYAPP> {SHEBANG}        { yybegin(LINE); return makeToken(SHEBANG); }
<INIT_ADJACENT_TYAPP> {HASH}"light"    { yybegin(LINE); return makeToken(PP_LIGHT); }

<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {PP_BAD_COMPILER_DIRECTIVE} { yypushback(yylength()); yybegin(BAD_PPSHARP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<LINE, ADJACENT_TYAPP> {LQUOTE_TYPED}   { return makeToken(LQUOTE_TYPED); }
<LINE, ADJACENT_TYAPP> {RQUOTE_TYPED}   { return makeToken(RQUOTE_TYPED); }
<LINE, ADJACENT_TYAPP> {LQUOTE_UNTYPED} { return makeToken(LQUOTE_UNTYPED); }
<LINE, ADJACENT_TYAPP> {RQUOTE_UNTYPED} { return makeToken(RQUOTE_UNTYPED); }

<LINE, INIT_ADJACENT_TYAPP> {LQUOTE_TYPED}   { yybegin(LINE); return makeToken(LQUOTE_TYPED); }
<LINE, INIT_ADJACENT_TYAPP> {RQUOTE_TYPED}   { yybegin(LINE); return makeToken(RQUOTE_TYPED); }
<LINE, INIT_ADJACENT_TYAPP> {LQUOTE_UNTYPED} { yybegin(LINE); return makeToken(LQUOTE_UNTYPED); }
<LINE, INIT_ADJACENT_TYAPP> {RQUOTE_UNTYPED} { yybegin(LINE); return makeToken(RQUOTE_UNTYPED); }

<LINE, ADJACENT_TYAPP> "@>."|"@@>." { yypushback(yylength()); initSmash(SMASH_RQUOTE_DOT_FROM_LINE, SMASH_RQUOTE_DOT); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<INIT_ADJACENT_TYAPP> "@>."|"@@>."  { yypushback(yylength()); yybegin(SMASH_RQUOTE_DOT_FROM_LINE); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<LINE, ADJACENT_TYAPP> {LET_BANG}            { return makeToken(LET_BANG); }
<LINE, ADJACENT_TYAPP> {USE_BANG}            { return makeToken(USE_BANG); }
<LINE, ADJACENT_TYAPP> {DO_BANG}             { return makeToken(DO_BANG); }
<LINE, ADJACENT_TYAPP> {YIELD_BANG}          { return makeToken(YIELD_BANG); }
<LINE, ADJACENT_TYAPP> {RETURN_BANG}         { return makeToken(RETURN_BANG); }
<LINE, ADJACENT_TYAPP> {MATCH_BANG}          { return makeToken(MATCH_BANG); }
<LINE, ADJACENT_TYAPP> {BAR}                 { return makeToken(BAR); }
<LINE, ADJACENT_TYAPP> {LARROW}              { return makeToken(LARROW); }
<LINE, ADJACENT_TYAPP> {LPAREN}              { return makeToken(LPAREN); }
<LINE, ADJACENT_TYAPP> {RPAREN}              { return makeToken(RPAREN); }
<LINE, ADJACENT_TYAPP> {LBRACK}              { return makeToken(LBRACK); }
<LINE, ADJACENT_TYAPP> {RBRACK}              { return makeToken(RBRACK); }
<LINE, ADJACENT_TYAPP> {LBRACK_LESS}         { return makeToken(LBRACK_LESS); }
<LINE, ADJACENT_TYAPP> {GREATER_RBRACK}      { return makeToken(GREATER_RBRACK); }
<LINE, ADJACENT_TYAPP> {LBRACK_BAR}          { return makeToken(LBRACK_BAR); }
<LINE, ADJACENT_TYAPP> {LESS}                { return makeToken(LESS); }
<LINE, ADJACENT_TYAPP> {GREATER}             { return makeToken(GREATER); }
<LINE, ADJACENT_TYAPP> {BAR_RBRACK}          { return makeToken(BAR_RBRACK); }
<LINE, ADJACENT_TYAPP> {LBRACE}              { return makeToken(LBRACE); }
<LINE, ADJACENT_TYAPP> {RBRACE}              { return makeToken(RBRACE); }
<LINE, ADJACENT_TYAPP> {GREATER_BAR_RBRACK}  { return makeToken(GREATER_BAR_RBRACK); }
<LINE, ADJACENT_TYAPP> {COLON_QMARK_GREATER} { return makeToken(COLON_QMARK_GREATER); }
<LINE, ADJACENT_TYAPP> {COLON_QMARK}         { return makeToken(COLON_QMARK); }
<LINE, ADJACENT_TYAPP> {COLON_COLON}         { return makeToken(COLON_COLON); }
<LINE, ADJACENT_TYAPP> {COLON_EQUALS}        { return makeToken(COLON_EQUALS); }
<LINE, ADJACENT_TYAPP> {SEMICOLON_SEMICOLON} { return makeToken(SEMICOLON_SEMICOLON); }
<LINE, ADJACENT_TYAPP> {SEMICOLON}           { return makeToken(SEMICOLON); }
<LINE, ADJACENT_TYAPP> {QMARK}               { return makeToken(QMARK); }
<LINE, ADJACENT_TYAPP> {QMARK_QMARK}         { return makeToken(QMARK_QMARK); }
<LINE, ADJACENT_TYAPP> {LPAREN_STAR_RPAREN}  { return makeToken(LPAREN_STAR_RPAREN); }
<LINE, ADJACENT_TYAPP> {PLUS}                { return makeToken(PLUS); }
<LINE, ADJACENT_TYAPP> {DOLLAR}              { return makeToken(DOLLAR); }
<LINE, ADJACENT_TYAPP> {PERCENT}             { return makeToken(PERCENT); }
<LINE, ADJACENT_TYAPP> {PERCENT_PERCENT}     { return makeToken(PERCENT_PERCENT); }
<LINE, ADJACENT_TYAPP> {AMP}                 { return makeToken(AMP); }
<LINE, ADJACENT_TYAPP> {AMP_AMP}             { return makeToken(AMP_AMP); }

<INIT_ADJACENT_TYAPP> {LET_BANG}            { yybegin(LINE); return makeToken(LET_BANG); }
<INIT_ADJACENT_TYAPP> {USE_BANG}            { yybegin(LINE); return makeToken(USE_BANG); }
<INIT_ADJACENT_TYAPP> {DO_BANG}             { yybegin(LINE); return makeToken(DO_BANG); }
<INIT_ADJACENT_TYAPP> {YIELD_BANG}          { yybegin(LINE); return makeToken(YIELD_BANG); }
<INIT_ADJACENT_TYAPP> {RETURN_BANG}         { yybegin(LINE); return makeToken(RETURN_BANG); }
<INIT_ADJACENT_TYAPP> {MATCH_BANG}          { yybegin(LINE); return makeToken(MATCH_BANG); }
<INIT_ADJACENT_TYAPP> {BAR}                 { yybegin(LINE); return makeToken(BAR); }
<INIT_ADJACENT_TYAPP> {LARROW}              { yybegin(LINE); return makeToken(LARROW); }
<INIT_ADJACENT_TYAPP> {LBRACK_BAR}          { yybegin(LINE); return makeToken(LBRACK_BAR); }
<INIT_ADJACENT_TYAPP> {BAR_RBRACK}          { yybegin(LINE); return makeToken(BAR_RBRACK); }
<INIT_ADJACENT_TYAPP> {LBRACE}              { yybegin(LINE); return makeToken(LBRACE); }
<INIT_ADJACENT_TYAPP> {RBRACE}              { yybegin(LINE); return makeToken(RBRACE); }
<INIT_ADJACENT_TYAPP> {COLON_QMARK_GREATER} { yybegin(LINE); return makeToken(COLON_QMARK_GREATER); }
<INIT_ADJACENT_TYAPP> {COLON_QMARK}         { yybegin(LINE); return makeToken(COLON_QMARK); }
<INIT_ADJACENT_TYAPP> {COLON_COLON}         { yybegin(LINE); return makeToken(COLON_COLON); }
<INIT_ADJACENT_TYAPP> {COLON_EQUALS}        { yybegin(LINE); return makeToken(COLON_EQUALS); }
<INIT_ADJACENT_TYAPP> {SEMICOLON_SEMICOLON} { yybegin(LINE); return makeToken(SEMICOLON_SEMICOLON); }
<INIT_ADJACENT_TYAPP> {QMARK}               { yybegin(LINE); return makeToken(QMARK); }
<INIT_ADJACENT_TYAPP> {QMARK_QMARK}         { yybegin(LINE); return makeToken(QMARK_QMARK); }
<INIT_ADJACENT_TYAPP> {LPAREN_STAR_RPAREN}  { yybegin(LINE); return makeToken(LPAREN_STAR_RPAREN); }
<INIT_ADJACENT_TYAPP> {PLUS}                { yybegin(LINE); return makeToken(PLUS); }
<INIT_ADJACENT_TYAPP> {DOLLAR}              { yybegin(LINE); return makeToken(DOLLAR); }
<INIT_ADJACENT_TYAPP> {PERCENT}             { yybegin(LINE); return makeToken(PERCENT); }
<INIT_ADJACENT_TYAPP> {PERCENT_PERCENT}     { yybegin(LINE); return makeToken(PERCENT_PERCENT); }
<INIT_ADJACENT_TYAPP> {AMP}                 { yybegin(LINE); return makeToken(AMP); }
<INIT_ADJACENT_TYAPP> {AMP_AMP}             { yybegin(LINE); return makeToken(AMP_AMP); }

<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {LBRACE_BAR}    { return makeToken(LBRACE_BAR); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {BAR_RBRACE}    { return makeToken(BAR_RBRACE); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {SEMICOLON}     { return makeToken(SEMICOLON); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {RARROW}        { return makeToken(RARROW); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {DOT}           { return makeToken(DOT); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {COLON}         { return makeToken(COLON); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {STAR}          { return makeToken(STAR); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {QUOTE}         { return makeToken(QUOTE); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {HASH}          { return makeToken(HASH); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {COLON_GREATER} { return makeToken(COLON_GREATER); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {DOT_DOT}       { return makeToken(DOT_DOT); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {EQUALS}        { return makeToken(EQUALS); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {UNDERSCORE}    { return makeToken(UNDERSCORE); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {MINUS}         { return makeToken(MINUS); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {COMMA}         { return makeToken(COMMA); }

<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {KEYWORD_STRING_SOURCE_DIRECTORY} { return makeToken(KEYWORD_STRING_SOURCE_DIRECTORY); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {KEYWORD_STRING_SOURCE_FILE}      { return makeToken(KEYWORD_STRING_SOURCE_FILE); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {KEYWORD_STRING_LINE}             { return makeToken(KEYWORD_STRING_LINE); }

<LINE, ADJACENT_TYAPP> {RESERVED_IDENT_FORMATS} { return makeToken(RESERVED_IDENT_FORMATS); }

<LINE> "delegate"{LESS_OP}                 { initAdjacentTypeApp(); return makeToken(DELEGATE); }
<LINE> {IDENT}{LESS_OP}                    { initAdjacentTypeApp(); return identInTypeApp(); }
<LINE> {IEEE32}{LESS_OP}                   { initAdjacentTypeApp(); return makeToken(IEEE32); }
<LINE> {IEEE64}{LESS_OP}                   { initAdjacentTypeApp(); return makeToken(IEEE64); }
<LINE> {DECIMAL}{LESS_OP}                  { initAdjacentTypeApp(); return makeToken(DECIMAL); }
<LINE> {BYTE}{LESS_OP}                     { initAdjacentTypeApp(); return makeToken(BYTE); }
<LINE> {INT16}{LESS_OP}                    { initAdjacentTypeApp(); return makeToken(INT16); }
<LINE> ({XINT}|{INT}){LESS_OP}             { initAdjacentTypeApp(); return makeToken(INT32); }
<LINE> {INT32}{LESS_OP}                    { initAdjacentTypeApp(); return makeToken(INT32); }
<LINE> {INT64}{LESS_OP}                    { initAdjacentTypeApp(); return makeToken(INT64); }
<LINE> {SBYTE}{LESS_OP}                    { initAdjacentTypeApp(); return makeToken(SBYTE); }
<LINE> {UINT16}{LESS_OP}                   { initAdjacentTypeApp(); return makeToken(UINT16); }
<LINE> {UINT32}{LESS_OP}                   { initAdjacentTypeApp(); return makeToken(UINT32); }
<LINE> {UINT64}{LESS_OP}                   { initAdjacentTypeApp(); return makeToken(UINT64); }
<LINE> {BIGNUM}{LESS_OP}                   { initAdjacentTypeApp(); return makeToken(BIGNUM); }
<LINE> {NATIVEINT}{LESS_OP}                { initAdjacentTypeApp(); return makeToken(NATIVEINT); }
<LINE> {RESERVED_LITERAL_FORMATS}{LESS_OP} { initAdjacentTypeApp(); return makeToken(RESERVED_LITERAL_FORMATS); }

<LINE> "delegate"{LESS_OP}{BAD_SYMBOLIC_OP}                 { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {IDENT}{LESS_OP}{BAD_SYMBOLIC_OP}                    { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {IEEE32}{LESS_OP}{BAD_SYMBOLIC_OP}                   { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {IEEE64}{LESS_OP}{BAD_SYMBOLIC_OP}                   { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {DECIMAL}{LESS_OP}{BAD_SYMBOLIC_OP}                  { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {BYTE}{LESS_OP}{BAD_SYMBOLIC_OP}                     { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {INT16}{LESS_OP}{BAD_SYMBOLIC_OP}                    { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> ({XINT}|{INT}){LESS_OP}{BAD_SYMBOLIC_OP}             { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {INT32}{LESS_OP}{BAD_SYMBOLIC_OP}                    { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {INT64}{LESS_OP}{BAD_SYMBOLIC_OP}                    { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {SBYTE}{LESS_OP}{BAD_SYMBOLIC_OP}                    { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {UINT16}{LESS_OP}{BAD_SYMBOLIC_OP}                   { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {UINT32}{LESS_OP}{BAD_SYMBOLIC_OP}                   { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {UINT64}{LESS_OP}{BAD_SYMBOLIC_OP}                   { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {BIGNUM}{LESS_OP}{BAD_SYMBOLIC_OP}                   { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {NATIVEINT}{LESS_OP}{BAD_SYMBOLIC_OP}                { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<LINE> {RESERVED_LITERAL_FORMATS}{LESS_OP}{BAD_SYMBOLIC_OP} { yypushback(yylength()); yybegin(PRE_LESS_OP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<PRE_LESS_OP> "delegate"                 { yybegin(LINE); return makeToken(DELEGATE); }
<PRE_LESS_OP> {IDENT}                    { yybegin(LINE); return identInTypeApp(); }
<PRE_LESS_OP> {IEEE32}                   { yybegin(LINE); return makeToken(IEEE32); }
<PRE_LESS_OP> {IEEE64}                   { yybegin(LINE); return makeToken(IEEE64); }
<PRE_LESS_OP> {DECIMAL}                  { yybegin(LINE); return makeToken(DECIMAL); }
<PRE_LESS_OP> {BYTE}                     { yybegin(LINE); return makeToken(BYTE); }
<PRE_LESS_OP> {INT16}                    { yybegin(LINE); return makeToken(INT16); }
<PRE_LESS_OP> {XINT}|{INT}               { yybegin(LINE); return makeToken(INT32); }
<PRE_LESS_OP> {INT32}                    { yybegin(LINE); return makeToken(INT32); }
<PRE_LESS_OP> {INT64}                    { yybegin(LINE); return makeToken(INT64); }
<PRE_LESS_OP> {SBYTE}                    { yybegin(LINE); return makeToken(SBYTE); }
<PRE_LESS_OP> {UINT16}                   { yybegin(LINE); return makeToken(UINT16); }
<PRE_LESS_OP> {UINT32}                   { yybegin(LINE); return makeToken(UINT32); }
<PRE_LESS_OP> {UINT64}                   { yybegin(LINE); return makeToken(UINT64); }
<PRE_LESS_OP> {BIGNUM}                   { yybegin(LINE); return makeToken(BIGNUM); }
<PRE_LESS_OP> {NATIVEINT}                { yybegin(LINE); return makeToken(NATIVEINT); }
<PRE_LESS_OP> {RESERVED_LITERAL_FORMATS} { yybegin(LINE); return makeToken(RESERVED_LITERAL_FORMATS); }

<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {IDENT}      { return initIdent(); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {SBYTE}      { return makeToken(SBYTE); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {BYTE}       { return makeToken(BYTE); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {INT16}      { return makeToken(INT16); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {UINT16}     { return makeToken(UINT16); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {XINT}|{INT} { return makeToken(INT32); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {INT32}      { return makeToken(INT32); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {UINT32}     { return makeToken(UINT32); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {NATIVEINT}  { return makeToken(NATIVEINT); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {UNATIVEINT} { return makeToken(UNATIVEINT); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {INT64}      { return makeToken(INT64); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {UINT64}     { return makeToken(UINT64); }

<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {INT}\.\. { yypushback(yylength()); initSmash(SMASH_INT_DOT_DOT_FROM_LINE, SMASH_INT_DOT_DOT); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {IEEE32}  { return makeToken(IEEE32); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {IEEE64}  { return makeToken(IEEE64); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {BIGNUM}  { return makeToken(BIGNUM); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {DECIMAL} { return makeToken(DECIMAL); }

<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {CHARACTER_LITERAL}               { return makeToken(CHARACTER_LITERAL); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {STRING}                          { return makeToken(STRING); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {UNFINISHED_STRING}               { return makeToken(UNFINISHED_STRING); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {VERBATIM_STRING}                 { return makeToken(VERBATIM_STRING); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {UNFINISHED_VERBATIM_STRING}      { return makeToken(UNFINISHED_VERBATIM_STRING); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {TRIPLE_QUOTED_STRING}            { return makeToken(TRIPLE_QUOTED_STRING); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {UNFINISHED_TRIPLE_QUOTED_STRING} { return makeToken(UNFINISHED_TRIPLE_QUOTED_STRING); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {BYTEARRAY}                       { return makeToken(BYTEARRAY); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {VERBATIM_BYTEARRAY}              { return makeToken(VERBATIM_BYTEARRAY); }
<LINE, ADJACENT_TYAPP, INIT_ADJACENT_TYAPP> {BYTECHAR}                        { return makeToken(BYTECHAR); }

<LINE, ADJACENT_TYAPP> {RESERVED_SYMBOLIC_SEQUENCE} { return makeToken(RESERVED_SYMBOLIC_SEQUENCE); }
<LINE, ADJACENT_TYAPP> {RESERVED_LITERAL_FORMATS}   { return makeToken(RESERVED_LITERAL_FORMATS); }
<LINE, ADJACENT_TYAPP> {INFIX_STAR_STAR_OP}         { return makeToken(INFIX_STAR_STAR_OP); }
<LINE, ADJACENT_TYAPP> {INFIX_STAR_DIV_MOD_OP}      { return makeToken(INFIX_STAR_DIV_MOD_OP); }
<LINE, ADJACENT_TYAPP> {PLUS_MINUS_OP}              { return makeToken(PLUS_MINUS_OP); }
<LINE, ADJACENT_TYAPP> {INFIX_AT_HAT_OP}            { return makeToken(INFIX_AT_HAT_OP); }
<LINE, ADJACENT_TYAPP> {INFIX_COMPARE_OP}           { return makeToken(INFIX_COMPARE_OP); }
<LINE, ADJACENT_TYAPP> {INFIX_AMP_OP}               { return makeToken(INFIX_AMP_OP); }
<LINE, ADJACENT_TYAPP> {INFIX_BAR_OP}               { return makeToken(INFIX_BAR_OP); }
<LINE, ADJACENT_TYAPP> {PREFIX_OP}                  { return makeToken(PREFIX_OP); }
<LINE, ADJACENT_TYAPP> {FUNKY_OPERATOR_NAME}        { return makeToken(PREFIX_OP); }
<LINE, ADJACENT_TYAPP> {SYMBOLIC_OP}                { return makeToken(SYMBOLIC_OP); }

<LINE, ADJACENT_TYAPP> {BAD_INFIX_STAR_STAR_OP}     { return makeToken(BAD_INFIX_STAR_STAR_OP); }
<LINE, ADJACENT_TYAPP> {BAD_INFIX_STAR_DIV_MOD_OP}  { return makeToken(BAD_INFIX_STAR_DIV_MOD_OP); }
<LINE, ADJACENT_TYAPP> {BAD_PLUS_MINUS_OP}          { return makeToken(BAD_PLUS_MINUS_OP); }
<LINE, ADJACENT_TYAPP> {BAD_INFIX_AT_HAT_OP}        { return makeToken(BAD_INFIX_AT_HAT_OP); }
<LINE, ADJACENT_TYAPP> {BAD_INFIX_COMPARE_OP}       { return makeToken(BAD_INFIX_COMPARE_OP); }
<LINE, ADJACENT_TYAPP> {BAD_INFIX_AMP_OP}           { return makeToken(BAD_INFIX_AMP_OP); }
<LINE, ADJACENT_TYAPP> {BAD_INFIX_BAR_OP}           { return makeToken(BAD_INFIX_BAR_OP); }
<LINE, ADJACENT_TYAPP> {BAD_PREFIX_OP}              { return makeToken(BAD_PREFIX_OP); }
<LINE, ADJACENT_TYAPP> {BAD_SYMBOLIC_OP}            { return makeToken(BAD_SYMBOLIC_OP); }

<INIT_ADJACENT_TYAPP> {RESERVED_SYMBOLIC_SEQUENCE} { yybegin(LINE); return makeToken(RESERVED_SYMBOLIC_SEQUENCE); }
<INIT_ADJACENT_TYAPP> {RESERVED_LITERAL_FORMATS}   { yybegin(LINE); return makeToken(RESERVED_LITERAL_FORMATS); }
<INIT_ADJACENT_TYAPP> {INFIX_STAR_STAR_OP}         { yybegin(LINE); return makeToken(INFIX_STAR_STAR_OP); }
<INIT_ADJACENT_TYAPP> {INFIX_STAR_DIV_MOD_OP}      { yybegin(LINE); return makeToken(INFIX_STAR_DIV_MOD_OP); }
<INIT_ADJACENT_TYAPP> {PLUS_MINUS_OP}              { yybegin(LINE); return makeToken(PLUS_MINUS_OP); }
<INIT_ADJACENT_TYAPP> {INFIX_AT_HAT_OP}            { yybegin(LINE); return makeToken(INFIX_AT_HAT_OP); }
<INIT_ADJACENT_TYAPP> {INFIX_COMPARE_OP}           { yybegin(LINE); return makeToken(INFIX_COMPARE_OP); }
<INIT_ADJACENT_TYAPP> {INFIX_AMP_OP}               { yybegin(LINE); return makeToken(INFIX_AMP_OP); }
<INIT_ADJACENT_TYAPP> {INFIX_BAR_OP}               { yybegin(LINE); return makeToken(INFIX_BAR_OP); }
<INIT_ADJACENT_TYAPP> {PREFIX_OP}                  { yybegin(LINE); return makeToken(PREFIX_OP); }
<INIT_ADJACENT_TYAPP> {FUNKY_OPERATOR_NAME}        { yybegin(LINE); return makeToken(PREFIX_OP); }
<INIT_ADJACENT_TYAPP> {SYMBOLIC_OP}                { yybegin(LINE); return makeToken(SYMBOLIC_OP); }

<INIT_ADJACENT_TYAPP> {BAD_INFIX_STAR_STAR_OP}     { yybegin(LINE); return makeToken(BAD_INFIX_STAR_STAR_OP); }
<INIT_ADJACENT_TYAPP> {BAD_INFIX_STAR_DIV_MOD_OP}  { yybegin(LINE); return makeToken(BAD_INFIX_STAR_DIV_MOD_OP); }
<INIT_ADJACENT_TYAPP> {BAD_PLUS_MINUS_OP}          { yybegin(LINE); return makeToken(BAD_PLUS_MINUS_OP); }
<INIT_ADJACENT_TYAPP> {BAD_INFIX_AT_HAT_OP}        { yybegin(LINE); return makeToken(BAD_INFIX_AT_HAT_OP); }
<INIT_ADJACENT_TYAPP> {BAD_INFIX_COMPARE_OP}       { yybegin(LINE); return makeToken(BAD_INFIX_COMPARE_OP); }
<INIT_ADJACENT_TYAPP> {BAD_INFIX_AMP_OP}           { yybegin(LINE); return makeToken(BAD_INFIX_AMP_OP); }
<INIT_ADJACENT_TYAPP> {BAD_INFIX_BAR_OP}           { yybegin(LINE); return makeToken(BAD_INFIX_BAR_OP); }
<INIT_ADJACENT_TYAPP> {BAD_PREFIX_OP}              { yybegin(LINE); return makeToken(BAD_PREFIX_OP); }
<INIT_ADJACENT_TYAPP> {BAD_SYMBOLIC_OP}            { yybegin(LINE); return makeToken(BAD_SYMBOLIC_OP); }

<SMASH_INT_DOT_DOT, SMASH_INT_DOT_DOT_FROM_LINE> {INT} { return makeToken(INT32); }
<SMASH_INT_DOT_DOT, SMASH_INT_DOT_DOT_FROM_LINE> \.\.  { exitSmash(SMASH_INT_DOT_DOT_FROM_LINE); return makeToken(DOT_DOT); }

<SMASH_RQUOTE_DOT, SMASH_RQUOTE_DOT_FROM_LINE> "@>"  { return makeToken(RQUOTE_TYPED); }
<SMASH_RQUOTE_DOT, SMASH_RQUOTE_DOT_FROM_LINE> "@@>" { return makeToken(RQUOTE_UNTYPED); }
<SMASH_RQUOTE_DOT, SMASH_RQUOTE_DOT_FROM_LINE> \.    { exitSmash(SMASH_RQUOTE_DOT_FROM_LINE); return makeToken(DOT); }

<STRING_IN_COMMENT> {STRING}                          { yybegin(IN_BLOCK_COMMENT); increaseTokenLength(yylength()); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<STRING_IN_COMMENT> {VERBATIM_STRING}                 { yybegin(IN_BLOCK_COMMENT); increaseTokenLength(yylength()); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<STRING_IN_COMMENT> {TRIPLE_QUOTED_STRING}            { yybegin(IN_BLOCK_COMMENT); increaseTokenLength(yylength()); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<STRING_IN_COMMENT> {UNFINISHED_STRING}               { return fillBlockComment(UNFINISHED_STRING_IN_COMMENT); }
<STRING_IN_COMMENT> {UNFINISHED_VERBATIM_STRING}      { return fillBlockComment(UNFINISHED_VERBATIM_STRING_IN_COMMENT); }
<STRING_IN_COMMENT> {UNFINISHED_TRIPLE_QUOTED_STRING} { return fillBlockComment(UNFINISHED_TRIPLE_QUOTED_STRING_IN_COMMENT); }

<IN_BLOCK_COMMENT, IN_BLOCK_COMMENT_FROM_LINE> "(*"      { zzNestedCommentLevel++; increaseTokenLength(yylength()); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<IN_BLOCK_COMMENT, IN_BLOCK_COMMENT_FROM_LINE> "*)"      { if (--zzNestedCommentLevel == 0) return fillBlockComment(BLOCK_COMMENT); increaseTokenLength(yylength()); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<IN_BLOCK_COMMENT, IN_BLOCK_COMMENT_FROM_LINE> \"        { yypushback(yylength()); yybegin(STRING_IN_COMMENT); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<IN_BLOCK_COMMENT, IN_BLOCK_COMMENT_FROM_LINE> @\"       { yypushback(yylength()); yybegin(STRING_IN_COMMENT); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<IN_BLOCK_COMMENT, IN_BLOCK_COMMENT_FROM_LINE> \"\"\"    { yypushback(yylength()); yybegin(STRING_IN_COMMENT); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<IN_BLOCK_COMMENT, IN_BLOCK_COMMENT_FROM_LINE> [^(\"@*]+ { increaseTokenLength(yylength()); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<IN_BLOCK_COMMENT, IN_BLOCK_COMMENT_FROM_LINE> [^]|"(*)" { increaseTokenLength(yylength()); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<IN_BLOCK_COMMENT, IN_BLOCK_COMMENT_FROM_LINE> <<EOF>>   { return fillBlockComment(UNFINISHED_BLOCK_COMMENT); }

<SMASH_ADJACENT_LESS_OP> {LESS} { return makeToken(LESS); }
<SMASH_ADJACENT_LESS_OP> "/"    { riseFromParenLevel(0); return makeToken(INFIX_STAR_DIV_MOD_OP); }

<SMASH_ADJACENT_GREATER_BAR_RBRACK, SMASH_ADJACENT_GREATER_BAR_RBRACK_FIN> {GREATER}    { return makeToken(GREATER); }
<SMASH_ADJACENT_GREATER_BAR_RBRACK, SMASH_ADJACENT_GREATER_BAR_RBRACK_FIN> {BAR_RBRACK} { exitSmash(SMASH_ADJACENT_GREATER_BAR_RBRACK_FIN); return makeToken(BAR_RBRACK); }

<SMASH_ADJACENT_GREATER_RBRACK, SMASH_ADJACENT_GREATER_RBRACK_FIN> {GREATER} { return makeToken(GREATER); }
<SMASH_ADJACENT_GREATER_RBRACK, SMASH_ADJACENT_GREATER_RBRACK_FIN> {RBRACK}  { exitSmash(SMASH_ADJACENT_GREATER_RBRACK_FIN); return makeToken(RBRACK); }

<PPSHARP, PPSYMBOL, PPDIRECTIVE> {ANYWHITE} { return makeToken (WHITESPACE); }

<PPDIRECTIVE> {HASH}("l"|"load")                 { yybegin(LINE); return makeToken(PP_LOAD); }
<PPDIRECTIVE> {HASH}("r"|"reference")            { yybegin(LINE); return makeToken(PP_REFERENCE); }
<PPDIRECTIVE> {HASH}("line"|({ANYWHITE})*[0-9]+) { yybegin(LINE); return makeToken(PP_LINE); }
<PPDIRECTIVE> {HASH}"help"                       { yybegin(LINE); return makeToken(PP_HELP); }
<PPDIRECTIVE> {HASH}"quit"                       { yybegin(LINE); return makeToken(PP_QUIT); }
<PPDIRECTIVE> {HASH}("light"|"indent")           { yybegin(LINE); return makeToken(PP_LIGHT); }
<PPDIRECTIVE> {HASH}"time"                       { yybegin(LINE); return makeToken(PP_TIME); }
<PPDIRECTIVE> {HASH}"I"                          { yybegin(LINE); return makeToken(PP_I); }
<PPDIRECTIVE> {HASH}"nowarn"                     { yybegin(LINE); return makeToken(PP_NOWARN); }

<PPDIRECTIVE> {HASH}"if"                         { yypushback(yylength()); yybegin(PPSHARP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<PPDIRECTIVE> {HASH}"else"                       { yypushback(yylength()); yybegin(PPSHARP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<PPDIRECTIVE> {HASH}"endif"                      { yypushback(yylength()); yybegin(PPSHARP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<PPDIRECTIVE> {HASH}"if"{TAIL_IDENT}             { yypushback(yylength()); yybegin(PPSHARP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<PPDIRECTIVE> {HASH}"else"{TAIL_IDENT}           { yypushback(yylength()); yybegin(PPSHARP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }
<PPDIRECTIVE> {HASH}"endif"{TAIL_IDENT}          { yypushback(yylength()); yybegin(PPSHARP); clear(out yy_lookahead, out yy_anchor, out yy_state, out yy_next_state, out yy_last_accept_state, out yy_initial, out yy_this_accept); break; }

<PPDIRECTIVE> {HASH}{IDENT}                      { yybegin(LINE); return makeToken(PP_DIRECTIVE); }

<PPSHARP> {HASH}"if"    { yybegin(PPSYMBOL); return makeToken(PP_IF_SECTION); }
<PPSHARP> {HASH}"else"  { yybegin(PPSYMBOL); return makeToken(PP_ELSE_SECTION); }
<PPSHARP> {HASH}"endif" { yybegin(PPSYMBOL); return makeToken(PP_ENDIF); }

<BAD_PPSHARP> {HASH}"if"    { yybegin(PPSYMBOL); return makeToken(PP_DIRECTIVE); }
<BAD_PPSHARP> {HASH}"else"  { yybegin(PPSYMBOL); return makeToken(PP_DIRECTIVE); }
<BAD_PPSHARP> {HASH}"endif" { yybegin(PPSYMBOL); return makeToken(PP_DIRECTIVE); }

<PPSHARP, BAD_PPSHARP> {HASH}"if"{TAIL_IDENT}    { yypushback(yylength() - 3); yybegin(LINE); return makeToken(PP_DIRECTIVE); }
<PPSHARP, BAD_PPSHARP> {HASH}"else"{TAIL_IDENT}  { yypushback(yylength() - 5); yybegin(PPSYMBOL); return makeToken(PP_DIRECTIVE); }
<PPSHARP, BAD_PPSHARP> {HASH}"endif"{TAIL_IDENT} { yypushback(yylength() - 6); yybegin(PPSYMBOL); return makeToken(PP_DIRECTIVE); }

<PPSYMBOL> "||"                    { return makeToken(PP_OR); }
<PPSYMBOL> "&&"                    { return makeToken(PP_AND); }
<PPSYMBOL> "!"                     { return makeToken(PP_NOT); }
<PPSYMBOL> "("                     { return makeToken(PP_LPAR); }
<PPSYMBOL> ")"                     { return makeToken(PP_RPAR); }
<PPSYMBOL> {PP_CONDITIONAL_SYMBOL} { return makeToken(PP_CONDITIONAL_SYMBOL); }

<PPSHARP, PPSYMBOL> {LINE_COMMENT} { return makeToken(LINE_COMMENT); }
<PPSHARP, PPSYMBOL> {NEW_LINE}     { yybegin(YYINITIAL); return makeToken(NEW_LINE); }

<PPSYMBOL> . { return makeToken(PP_BAD_CHARACTER); }

<IN_BLOCK_COMMENT,
 IN_BLOCK_COMMENT_FROM_LINE,
 STRING_IN_COMMENT,
 SMASH_INT_DOT_DOT,
 SMASH_INT_DOT_DOT_FROM_LINE,
 SMASH_RQUOTE_DOT,
 SMASH_RQUOTE_DOT_FROM_LINE,
 SMASH_ADJACENT_LESS_OP,
 SMASH_ADJACENT_GREATER_BAR_RBRACK,
 SMASH_ADJACENT_GREATER_RBRACK,
 SMASH_ADJACENT_GREATER_BAR_RBRACK_FIN,
 SMASH_ADJACENT_GREATER_RBRACK_FIN,
 ADJACENT_TYPE_CLOSE_OP,
 INIT_ADJACENT_TYAPP,
 ADJACENT_TYAPP,
 SYMBOLIC_OPERATOR,
 GREATER_OP,
 GREATER_OP_SYMBOLIC_OP,
 PRE_LESS_OP,
 LINE> [^] { return makeToken(BAD_CHARACTER); }
