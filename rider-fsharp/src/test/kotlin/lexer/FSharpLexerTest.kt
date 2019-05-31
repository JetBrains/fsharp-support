package lexer

import com.jetbrains.rider.ideaInterop.fileTypes.fsharp.lexer.*
import com.intellij.lexer.Lexer
import com.intellij.testFramework.LexerTestCase
import org.testng.annotations.Test

@Test
class FSharpLexerTest : LexerTestCase() {
    override fun createLexer(): Lexer {
        return FSharpLexer()
    }

    override fun getDirPath(): String? {
        return null
    }

    @Test
    fun testDigit() {
        doTest("1234567890 1234567890u 1234567890l 0XABCDEFy 0x001100010s 3.0F 0x0000000000000000LF 34742626263193832612536171N 0o7 0b1 1F",
                """
                |INT32 ('1234567890')
                |WHITESPACE (' ')
                |UINT32 ('1234567890u')
                |WHITESPACE (' ')
                |INT32 ('1234567890l')
                |WHITESPACE (' ')
                |SBYTE ('0XABCDEFy')
                |WHITESPACE (' ')
                |INT16 ('0x001100010s')
                |WHITESPACE (' ')
                |IEEE32 ('3.0F')
                |WHITESPACE (' ')
                |IEEE64 ('0x0000000000000000LF')
                |WHITESPACE (' ')
                |BIGNUM ('34742626263193832612536171N')
                |WHITESPACE (' ')
                |INT32 ('0o7')
                |WHITESPACE (' ')
                |INT32 ('0b1')
                |WHITESPACE (' ')
                |RESERVED_LITERAL_FORMATS ('1F')
                """.trimMargin()
        )
    }

    @Test
    fun testString() {
        doTest("\"STRING\n \\ NEW_LINE\n\"",
                """STRING ('"STRING\n \ NEW_LINE\n"')"""
        )
    }

    @Test
    fun testVerbatimString() {
        doTest("@\"VERBATIM STRING\"",
                """VERBATIM_STRING ('@"VERBATIM STRING"')"""
        )
    }

    @Test
    fun testByteArray() {
        doTest("\"ByteArray\"B",
                """BYTEARRAY ('"ByteArray"B')"""
        )
    }

    @Test
    fun testTripleQuotedString() {
        doTest("\"\"\"triple-quoted-string \ntriple-quoted-string\"\"\"",
                "TRIPLE_QUOTED_STRING ('\"\"\"triple-quoted-string \\ntriple-quoted-string\"\"\"')"
        )
    }

    @Test
    fun testSymbolicOperator() {
        doTest("&&& ||| ?<- @-><-= ?-> >-> >>= >>- |> .>>. .>> >>",
                """
                |INFIX_AMP_OP ('&&&')
                |WHITESPACE (' ')
                |INFIX_BAR_OP ('|||')
                |WHITESPACE (' ')
                |INFIX_COMPARE_OP ('?<-')
                |WHITESPACE (' ')
                |INFIX_AT_HAT_OP ('@-><-=')
                |WHITESPACE (' ')
                |PLUS_MINUS_OP ('?->')
                |WHITESPACE (' ')
                |INFIX_COMPARE_OP ('>->')
                |WHITESPACE (' ')
                |INFIX_COMPARE_OP ('>>=')
                |WHITESPACE (' ')
                |INFIX_COMPARE_OP ('>>-')
                |WHITESPACE (' ')
                |INFIX_BAR_OP ('|>')
                |WHITESPACE (' ')
                |INFIX_COMPARE_OP ('.>>.')
                |WHITESPACE (' ')
                |INFIX_COMPARE_OP ('.>>')
                |WHITESPACE (' ')
                |INFIX_COMPARE_OP ('>>')
                """.trimMargin()
        )
    }

    @Test
    fun testSimpleBlockComment() {
        doTest("(* HAHA *)", "BLOCK_COMMENT ('(* HAHA *)')")
    }

    @Test
    fun testBlockComment() {
        doTest("(* Here's a code snippet: let s = \"*)\" *)",
                """BLOCK_COMMENT ('(* Here's a code snippet: let s = "*)" *)')"""
        )
    }

    @Test
    fun testBlockCommentError() {
        doTest("(* \" *)",
                "UNFINISHED_STRING_IN_COMMENT ('(* \" *)')"
        )
    }

    @Test
    fun testSymbolicKeyword() {
        doTest("let! use! do! yield! return! match! | -> <- . : ( ) [ ] [< >] " +
                "[| |] { } ' # :?> :? :> .. :: := ;; ; = _ ? ?? (*) <@ @> <@@ @@>",
                """
                |LET_BANG ('let!')
                |WHITESPACE (' ')
                |USE_BANG ('use!')
                |WHITESPACE (' ')
                |DO_BANG ('do!')
                |WHITESPACE (' ')
                |YIELD_BANG ('yield!')
                |WHITESPACE (' ')
                |RETURN_BANG ('return!')
                |WHITESPACE (' ')
                |MATCH_BANG ('match!')
                |WHITESPACE (' ')
                |BAR ('|')
                |WHITESPACE (' ')
                |RARROW ('->')
                |WHITESPACE (' ')
                |LARROW ('<-')
                |WHITESPACE (' ')
                |DOT ('.')
                |WHITESPACE (' ')
                |COLON (':')
                |WHITESPACE (' ')
                |LPAREN ('(')
                |WHITESPACE (' ')
                |RPAREN (')')
                |WHITESPACE (' ')
                |LBRACK ('[')
                |WHITESPACE (' ')
                |RBRACK (']')
                |WHITESPACE (' ')
                |LBRACK_LESS ('[<')
                |WHITESPACE (' ')
                |GREATER_RBRACK ('>]')
                |WHITESPACE (' ')
                |LBRACK_BAR ('[|')
                |WHITESPACE (' ')
                |BAR_RBRACK ('|]')
                |WHITESPACE (' ')
                |LBRACE ('{')
                |WHITESPACE (' ')
                |RBRACE ('}')
                |WHITESPACE (' ')
                |QUOTE (''')
                |WHITESPACE (' ')
                |HASH ('#')
                |WHITESPACE (' ')
                |COLON_QMARK_GREATER (':?>')
                |WHITESPACE (' ')
                |COLON_QMARK (':?')
                |WHITESPACE (' ')
                |COLON_GREATER (':>')
                |WHITESPACE (' ')
                |DOT_DOT ('..')
                |WHITESPACE (' ')
                |COLON_COLON ('::')
                |WHITESPACE (' ')
                |COLON_EQUALS (':=')
                |WHITESPACE (' ')
                |SEMICOLON_SEMICOLON (';;')
                |WHITESPACE (' ')
                |SEMICOLON (';')
                |WHITESPACE (' ')
                |EQUALS ('=')
                |WHITESPACE (' ')
                |UNDERSCORE ('_')
                |WHITESPACE (' ')
                |QMARK ('?')
                |WHITESPACE (' ')
                |QMARK_QMARK ('??')
                |WHITESPACE (' ')
                |LPAREN_STAR_RPAREN ('(*)')
                |WHITESPACE (' ')
                |LQUOTE_TYPED ('<@')
                |WHITESPACE (' ')
                |RQUOTE_TYPED ('@>')
                |WHITESPACE (' ')
                |LQUOTE_UNTYPED ('<@@')
                |WHITESPACE (' ')
                |RQUOTE_UNTYPED ('@@>')
                """.trimMargin()
        )
    }

    @Test
    fun testStringEscapeChar() {
        doTest("\"\\n\\t\\b\\r\\a\\f\\v\"", "STRING ('\"\\n\\t\\b\\r\\a\\f\\v\"')")
    }

    @Test
    fun testEscapeChar() {
        doTest("'\\n' '\\t' '\\b' '\\r' '\"' '\\a' '\\f' '\\v'",
                """
                |CHARACTER_LITERAL (''\n'')
                |WHITESPACE (' ')
                |CHARACTER_LITERAL (''\t'')
                |WHITESPACE (' ')
                |CHARACTER_LITERAL (''\b'')
                |WHITESPACE (' ')
                |CHARACTER_LITERAL (''\r'')
                |WHITESPACE (' ')
                |CHARACTER_LITERAL (''"'')
                |WHITESPACE (' ')
                |CHARACTER_LITERAL (''\a'')
                |WHITESPACE (' ')
                |CHARACTER_LITERAL (''\f'')
                |WHITESPACE (' ')
                |CHARACTER_LITERAL (''\v'')
                """.trimMargin()
        )
    }

    @Test
    fun testKeywordString() {
        doTest("__SOURCE_FILE__ __SOURCE_DIRECTORY__ __LINE__",
                """
                |KEYWORD_STRING_SOURCE_FILE ('__SOURCE_FILE__')
                |WHITESPACE (' ')
                |KEYWORD_STRING_SOURCE_DIRECTORY ('__SOURCE_DIRECTORY__')
                |WHITESPACE (' ')
                |KEYWORD_STRING_LINE ('__LINE__')
                """.trimMargin()
        )
    }

    @Test
    fun testUnfinishedTripleQuoteStringInComment() {
        doTest("(* \"\"\" *)\n" +
                """
                |let str = "STRING
                |\ NEW_LINE
                |"
                """.trimMargin(),
                "UNFINISHED_TRIPLE_QUOTED_STRING_IN_COMMENT ('(* \"\"\" *)\\nlet str = \"STRING\\n\\ NEW_LINE\\n\"')"
        )
    }

    @Test
    fun testIntDotDot() {
        doTest("1..20",
                """
                |INT32 ('1')
                |DOT_DOT ('..')
                |INT32 ('20')
                """.trimMargin()
        )
    }

    @Test
    fun testIdent() {
        doTest("``value.with odd#name``",
                "IDENT ('``value.with odd#name``')")
    }

    @Test
    fun testEndOfLineComment() {
        doTest("//hello world!\n//hello second world!",
                """
                |LINE_COMMENT ('//hello world!')
                |NEW_LINE ('\n')
                |LINE_COMMENT ('//hello second world!')
                """.trimMargin()
        )
    }

    @Test
    fun testUnfinishedString() {
        doTest("(* \"\"\"hello\"\"\" *)\n" +
                "        let str = \"STRING\n",
                "BLOCK_COMMENT ('(* \"\"\"hello\"\"\" *)')\n" +
                        """
                        |NEW_LINE ('\n')
                        |WHITESPACE ('        ')
                        |LET ('let')
                        |WHITESPACE (' ')
                        |IDENT ('str')
                        |WHITESPACE (' ')
                        |EQUALS ('=')
                        |WHITESPACE (' ')
                        |UNFINISHED_STRING ('"STRING\n')
                        """.trimMargin()
        )
    }

    @Test
    fun testCodeQuotation() {
        doTest("<@ 1 + 1 @>.ToString() <@@ 1 + 1 @@>.ToString()",
                """
                |LQUOTE_TYPED ('<@')
                |WHITESPACE (' ')
                |INT32 ('1')
                |WHITESPACE (' ')
                |PLUS ('+')
                |WHITESPACE (' ')
                |INT32 ('1')
                |WHITESPACE (' ')
                |RQUOTE_TYPED ('@>')
                |DOT ('.')
                |IDENT ('ToString')
                |LPAREN ('(')
                |RPAREN (')')
                |WHITESPACE (' ')
                |LQUOTE_UNTYPED ('<@@')
                |WHITESPACE (' ')
                |INT32 ('1')
                |WHITESPACE (' ')
                |PLUS ('+')
                |WHITESPACE (' ')
                |INT32 ('1')
                |WHITESPACE (' ')
                |RQUOTE_UNTYPED ('@@>')
                |DOT ('.')
                |IDENT ('ToString')
                |LPAREN ('(')
                |RPAREN (')')
                """.trimMargin()
        )
    }

    @Test
    fun testBadOperator() {
        doTest(".?:%? .?$%?",
                """
                |BAD_SYMBOLIC_OP ('.?:%?')
                |WHITESPACE (' ')
                |BAD_INFIX_STAR_DIV_MOD_OP ('.?${'$'}%?')
                """.trimMargin()
        )
    }

    @Test
    fun testAttribute() {
        doTest("[<SomeAttribute>]",
                """
                |LBRACK_LESS ('[<')
                |IDENT ('SomeAttribute')
                |GREATER_RBRACK ('>]')
                """.trimMargin()
        )
    }

    @Test
    fun testTypeApp() {
        doTest("let typeApp = typeof<Map<Map<Map<Map<_,_>[],Map<_,_[]>>,_>,_>>.FullName",
                """
                |LET ('let')
                |WHITESPACE (' ')
                |IDENT ('typeApp')
                |WHITESPACE (' ')
                |EQUALS ('=')
                |WHITESPACE (' ')
                |IDENT ('typeof')
                |LESS ('<')
                |IDENT ('Map')
                |LESS ('<')
                |IDENT ('Map')
                |LESS ('<')
                |IDENT ('Map')
                |LESS ('<')
                |IDENT ('Map')
                |LESS ('<')
                |IDENT ('_')
                |COMMA (',')
                |IDENT ('_')
                |GREATER ('>')
                |LBRACK ('[')
                |RBRACK (']')
                |COMMA (',')
                |IDENT ('Map')
                |LESS ('<')
                |IDENT ('_')
                |COMMA (',')
                |IDENT ('_')
                |LBRACK ('[')
                |RBRACK (']')
                |GREATER ('>')
                |GREATER ('>')
                |COMMA (',')
                |IDENT ('_')
                |GREATER ('>')
                |COMMA (',')
                |IDENT ('_')
                |GREATER ('>')
                |GREATER ('>')
                |DOT ('.')
                |IDENT ('FullName')
                """.trimMargin()
        )
    }

    @Test
    fun testCorrectTypeApp() {
        doTest("[typeof<int>] [typeof<int >]",
                """
                |LBRACK ('[')
                |IDENT ('typeof')
                |LESS ('<')
                |IDENT ('int')
                |GREATER ('>')
                |RBRACK (']')
                |WHITESPACE (' ')
                |LBRACK ('[')
                |IDENT ('typeof')
                |LESS ('<')
                |IDENT ('int')
                |WHITESPACE (' ')
                |GREATER ('>')
                |RBRACK (']')
                """.trimMargin()
        )
    }

    @Test
    fun testIncorrectTypeApp() {
        doTest("C<M<int >] >>> C<M<int >] > >>",
                """
                |IDENT ('C')
                |LESS ('<')
                |IDENT ('M')
                |LESS ('<')
                |IDENT ('int')
                |WHITESPACE (' ')
                |GREATER ('>')
                |RBRACK (']')
                |WHITESPACE (' ')
                |GREATER ('>')
                |GREATER ('>')
                |GREATER ('>')
                |WHITESPACE (' ')
                |IDENT ('C')
                |LESS ('<')
                |IDENT ('M')
                |LESS ('<')
                |IDENT ('int')
                |WHITESPACE (' ')
                |GREATER ('>')
                |RBRACK (']')
                |WHITESPACE (' ')
                |GREATER ('>')
                |WHITESPACE (' ')
                |INFIX_COMPARE_OP ('>>')
                """.trimMargin()
        )
    }

    @Test
    fun testGenericDeclaration() {
        doTest("""
            |type U<'a> = Choice1 of 'a
            |type 'a F = | F of 'a
            """.trimMargin(),
                """
                |TYPE ('type')
                |WHITESPACE (' ')
                |IDENT ('U')
                |LESS ('<')
                |QUOTE (''')
                |IDENT ('a')
                |GREATER ('>')
                |WHITESPACE (' ')
                |EQUALS ('=')
                |WHITESPACE (' ')
                |IDENT ('Choice1')
                |WHITESPACE (' ')
                |OF ('of')
                |WHITESPACE (' ')
                |QUOTE (''')
                |IDENT ('a')
                |NEW_LINE ('\n')
                |TYPE ('type')
                |WHITESPACE (' ')
                |QUOTE (''')
                |IDENT ('a')
                |WHITESPACE (' ')
                |IDENT ('F')
                |WHITESPACE (' ')
                |EQUALS ('=')
                |WHITESPACE (' ')
                |BAR ('|')
                |WHITESPACE (' ')
                |IDENT ('F')
                |WHITESPACE (' ')
                |OF ('of')
                |WHITESPACE (' ')
                |QUOTE (''')
                |IDENT ('a')
                """.trimMargin()
        )
    }

    @Test
    fun testIfDirective() {
        doTest("""
            |#if
            |#if (symbol || symbol) && symbol
            |#ifs symbol <- symbol
            """.trimMargin(),
                """
                    |PP_IF_SECTION ('#if')
                    |NEW_LINE ('\n')
                    |PP_IF_SECTION ('#if')
                    |WHITESPACE (' ')
                    |PP_LPAR ('(')
                    |PP_CONDITIONAL_SYMBOL ('symbol')
                    |WHITESPACE (' ')
                    |PP_OR ('||')
                    |WHITESPACE (' ')
                    |PP_CONDITIONAL_SYMBOL ('symbol')
                    |PP_RPAR (')')
                    |WHITESPACE (' ')
                    |PP_AND ('&&')
                    |WHITESPACE (' ')
                    |PP_CONDITIONAL_SYMBOL ('symbol')
                    |NEW_LINE ('\n')
                    |PP_DIRECTIVE ('#if')
                    |IDENT ('s')
                    |WHITESPACE (' ')
                    |IDENT ('symbol')
                    |WHITESPACE (' ')
                    |LARROW ('<-')
                    |WHITESPACE (' ')
                    |IDENT ('symbol')
                    """.trimMargin()
        )
    }

    @Test
    fun testElseDirective() {
        doTest("""
            |#else (symbol || symbol) && symbol
            |#else
            |#elses symbol
            """.trimMargin(),
                """
                |PP_ELSE_SECTION ('#else')
                |WHITESPACE (' ')
                |PP_LPAR ('(')
                |PP_CONDITIONAL_SYMBOL ('symbol')
                |WHITESPACE (' ')
                |PP_OR ('||')
                |WHITESPACE (' ')
                |PP_CONDITIONAL_SYMBOL ('symbol')
                |PP_RPAR (')')
                |WHITESPACE (' ')
                |PP_AND ('&&')
                |WHITESPACE (' ')
                |PP_CONDITIONAL_SYMBOL ('symbol')
                |NEW_LINE ('\n')
                |PP_ELSE_SECTION ('#else')
                |NEW_LINE ('\n')
                |PP_DIRECTIVE ('#else')
                |PP_CONDITIONAL_SYMBOL ('s')
                |WHITESPACE (' ')
                |PP_CONDITIONAL_SYMBOL ('symbol')
                """.trimMargin()
        )
    }

    @Test
    fun testEndIfDirective() {
        doTest("""
            |#endif
            |#endif (symbol || symbol) && symbol
            |#endifs symbol
            """.trimMargin(),
                """
                |PP_ENDIF ('#endif')
                |NEW_LINE ('\n')
                |PP_ENDIF ('#endif')
                |WHITESPACE (' ')
                |PP_LPAR ('(')
                |PP_CONDITIONAL_SYMBOL ('symbol')
                |WHITESPACE (' ')
                |PP_OR ('||')
                |WHITESPACE (' ')
                |PP_CONDITIONAL_SYMBOL ('symbol')
                |PP_RPAR (')')
                |WHITESPACE (' ')
                |PP_AND ('&&')
                |WHITESPACE (' ')
                |PP_CONDITIONAL_SYMBOL ('symbol')
                |NEW_LINE ('\n')
                |PP_DIRECTIVE ('#endif')
                |PP_CONDITIONAL_SYMBOL ('s')
                |WHITESPACE (' ')
                |PP_CONDITIONAL_SYMBOL ('symbol')
                """.trimMargin()
        )
    }

    @Test
    fun testLightDirective() {
        doTest("""
            |#light
            |#light "on"
            |#light "off"
            |#indent
            |#indent "on"
            |#indent "off"
            |let app : #light -> light = Constr >> Constr
            """.trimMargin(),
                """
                |PP_LIGHT ('#light')
                |NEW_LINE ('\n')
                |PP_LIGHT ('#light')
                |WHITESPACE (' ')
                |STRING ('"on"')
                |NEW_LINE ('\n')
                |PP_LIGHT ('#light')
                |WHITESPACE (' ')
                |STRING ('"off"')
                |NEW_LINE ('\n')
                |PP_LIGHT ('#indent')
                |NEW_LINE ('\n')
                |PP_LIGHT ('#indent')
                |WHITESPACE (' ')
                |STRING ('"on"')
                |NEW_LINE ('\n')
                |PP_LIGHT ('#indent')
                |WHITESPACE (' ')
                |STRING ('"off"')
                |NEW_LINE ('\n')
                |LET ('let')
                |WHITESPACE (' ')
                |IDENT ('app')
                |WHITESPACE (' ')
                |COLON (':')
                |WHITESPACE (' ')
                |PP_LIGHT ('#light')
                |WHITESPACE (' ')
                |RARROW ('->')
                |WHITESPACE (' ')
                |IDENT ('light')
                |WHITESPACE (' ')
                |EQUALS ('=')
                |WHITESPACE (' ')
                |IDENT ('Constr')
                |WHITESPACE (' ')
                |INFIX_COMPARE_OP ('>>')
                |WHITESPACE (' ')
                |IDENT ('Constr')
                """.trimMargin()
        )
    }

    @Test
    fun testLineDirective() {
        doTest("""
            |#line
            |#line "string"
            |#line @"verbatim string"
            |#44
            |#44 "string"
            |#44 @"verbatim string"
            |# 44
            |# 44 "string"
            |# 44 @"verbatim string"
            """.trimMargin(),
                """
                |PP_LINE ('#line')
                |NEW_LINE ('\n')
                |PP_LINE ('#line')
                |WHITESPACE (' ')
                |STRING ('"string"')
                |NEW_LINE ('\n')
                |PP_LINE ('#line')
                |WHITESPACE (' ')
                |VERBATIM_STRING ('@"verbatim string"')
                |NEW_LINE ('\n')
                |PP_LINE ('#44')
                |NEW_LINE ('\n')
                |PP_LINE ('#44')
                |WHITESPACE (' ')
                |STRING ('"string"')
                |NEW_LINE ('\n')
                |PP_LINE ('#44')
                |WHITESPACE (' ')
                |VERBATIM_STRING ('@"verbatim string"')
                |NEW_LINE ('\n')
                |PP_LINE ('# 44')
                |NEW_LINE ('\n')
                |PP_LINE ('# 44')
                |WHITESPACE (' ')
                |STRING ('"string"')
                |NEW_LINE ('\n')
                |PP_LINE ('# 44')
                |WHITESPACE (' ')
                |VERBATIM_STRING ('@"verbatim string"')
                """.trimMargin()
        )
    }

    @Test
    fun testBadCommentInDirective() {
        doTest("""
            |#if asdfasdf /*asdfasdfasdfasdfasdfasdf*/ asdfasdf
            |sadfsadf
            |#else
            |sadfasdf
            |#endif
            """.trimMargin(),
                """
                |PP_IF_SECTION ('#if')
                |WHITESPACE (' ')
                |PP_CONDITIONAL_SYMBOL ('asdfasdf')
                |WHITESPACE (' ')
                |PP_BAD_CHARACTER ('/')
                |PP_BAD_CHARACTER ('*')
                |PP_CONDITIONAL_SYMBOL ('asdfasdfasdfasdfasdfasdf')
                |PP_BAD_CHARACTER ('*')
                |PP_BAD_CHARACTER ('/')
                |WHITESPACE (' ')
                |PP_CONDITIONAL_SYMBOL ('asdfasdf')
                |NEW_LINE ('\n')
                |IDENT ('sadfsadf')
                |NEW_LINE ('\n')
                |PP_ELSE_SECTION ('#else')
                |NEW_LINE ('\n')
                |IDENT ('sadfasdf')
                |NEW_LINE ('\n')
                |PP_ENDIF ('#endif')
                """.trimMargin()
        )
    }

    @Test
    fun testEscapeCharacterInString() {
        doTest(""""\\" ()""",
                """
                |STRING ('"\\"')
                |WHITESPACE (' ')
                |LPAREN ('(')
                |RPAREN (')')
                """.trimMargin()
        )
    }

    @Test
    fun testEscapeCharacterInTripleQuotedString() {
        doTest("""""${'"'}\""${'"'} ()""",
                """
                |TRIPLE_QUOTED_STRING ('""${'"'}\""${'"'}')
                |WHITESPACE (' ')
                |LPAREN ('(')
                |RPAREN (')')
                """.trimMargin()
        )
    }

    @Test
    fun testEscapeCharacterInVerbatimString() {
        doTest("""@"\" ()""",
                """
                |VERBATIM_STRING ('@"\"')
                |WHITESPACE (' ')
                |LPAREN ('(')
                |RPAREN (')')
                """.trimMargin()
        )
    }

    @Test
    fun testInteractiveDirective() {
        doTest("""
            |#r "file.dll";;
            |#I "path";;
            |#load "file.fs" "file.fs";;
            |#time "on" "off";;
            |#help;;
            |#quit;;
            """.trimMargin(),
                """
                |PP_REFERENCE ('#r')
                |WHITESPACE (' ')
                |STRING ('"file.dll"')
                |SEMICOLON_SEMICOLON (';;')
                |NEW_LINE ('\n')
                |PP_I ('#I')
                |WHITESPACE (' ')
                |STRING ('"path"')
                |SEMICOLON_SEMICOLON (';;')
                |NEW_LINE ('\n')
                |PP_LOAD ('#load')
                |WHITESPACE (' ')
                |STRING ('"file.fs"')
                |WHITESPACE (' ')
                |STRING ('"file.fs"')
                |SEMICOLON_SEMICOLON (';;')
                |NEW_LINE ('\n')
                |PP_TIME ('#time')
                |WHITESPACE (' ')
                |STRING ('"on"')
                |WHITESPACE (' ')
                |STRING ('"off"')
                |SEMICOLON_SEMICOLON (';;')
                |NEW_LINE ('\n')
                |PP_HELP ('#help')
                |SEMICOLON_SEMICOLON (';;')
                |NEW_LINE ('\n')
                |PP_QUIT ('#quit')
                |SEMICOLON_SEMICOLON (';;')
                """.trimMargin()
        )
    }

    @Test
    fun testHelpQuitDirective() {
        doTest("""
            |#help
            |#quit
            """.trimMargin(),
                """
                |PP_HELP ('#help')
                |NEW_LINE ('\n')
                |PP_QUIT ('#quit')
                """.trimMargin()
        )
    }

    @Test
    fun testLoadDirective() {
        doTest("""
            |#l
            |#l "string"
            |#l @"verbatim string"
            |#load
            |#load "string"
            |#load @"verbatim string"
            """.trimMargin(),
                """
                |PP_LOAD ('#l')
                |NEW_LINE ('\n')
                |PP_LOAD ('#l')
                |WHITESPACE (' ')
                |STRING ('"string"')
                |NEW_LINE ('\n')
                |PP_LOAD ('#l')
                |WHITESPACE (' ')
                |VERBATIM_STRING ('@"verbatim string"')
                |NEW_LINE ('\n')
                |PP_LOAD ('#load')
                |NEW_LINE ('\n')
                |PP_LOAD ('#load')
                |WHITESPACE (' ')
                |STRING ('"string"')
                |NEW_LINE ('\n')
                |PP_LOAD ('#load')
                |WHITESPACE (' ')
                |VERBATIM_STRING ('@"verbatim string"')
                """.trimMargin()
        )
    }

    @Test
    fun testReferenceDirective() {
        doTest("""
            |#r
            |#r "string"
            |#r @"verbatim string"
            |#reference
            |#reference "string"
            |#reference @"verbatim string"
            """.trimMargin(),
                """
                |PP_REFERENCE ('#r')
                |NEW_LINE ('\n')
                |PP_REFERENCE ('#r')
                |WHITESPACE (' ')
                |STRING ('"string"')
                |NEW_LINE ('\n')
                |PP_REFERENCE ('#r')
                |WHITESPACE (' ')
                |VERBATIM_STRING ('@"verbatim string"')
                |NEW_LINE ('\n')
                |PP_REFERENCE ('#reference')
                |NEW_LINE ('\n')
                |PP_REFERENCE ('#reference')
                |WHITESPACE (' ')
                |STRING ('"string"')
                |NEW_LINE ('\n')
                |PP_REFERENCE ('#reference')
                |WHITESPACE (' ')
                |VERBATIM_STRING ('@"verbatim string"')
                """.trimMargin()
        )
    }

    @Test
    fun testTimeDirective() {
        doTest("""
            |#time
            |#time "string"
            |#time @"verbatim string"
            """.trimMargin(),
                """
                |PP_TIME ('#time')
                |NEW_LINE ('\n')
                |PP_TIME ('#time')
                |WHITESPACE (' ')
                |STRING ('"string"')
                |NEW_LINE ('\n')
                |PP_TIME ('#time')
                |WHITESPACE (' ')
                |VERBATIM_STRING ('@"verbatim string"')
                """.trimMargin()
        )
    }

    @Test
    fun testIDirective() {
        doTest("""
            |#I
            |#I "string"
            |#I @"verbatim string"
            """.trimMargin(),
                """
                |PP_I ('#I')
                |NEW_LINE ('\n')
                |PP_I ('#I')
                |WHITESPACE (' ')
                |STRING ('"string"')
                |NEW_LINE ('\n')
                |PP_I ('#I')
                |WHITESPACE (' ')
                |VERBATIM_STRING ('@"verbatim string"')
                """.trimMargin()
        )
    }

    @Test
    fun testSpaceDirective() {
        doTest(" #r \"on\"",
                """
                |WHITESPACE (' ')
                |PP_REFERENCE ('#r')
                |WHITESPACE (' ')
                |STRING ('"on"')
                """.trimMargin()
        )
    }

    @Test
    fun testFlexibleType() {
        doTest("let app : #r\nlet app1 : #if_",
                """
                |LET ('let')
                |WHITESPACE (' ')
                |IDENT ('app')
                |WHITESPACE (' ')
                |COLON (':')
                |WHITESPACE (' ')
                |HASH ('#')
                |IDENT ('r')
                |NEW_LINE ('\n')
                |LET ('let')
                |WHITESPACE (' ')
                |IDENT ('app1')
                |WHITESPACE (' ')
                |COLON (':')
                |WHITESPACE (' ')
                |PP_DIRECTIVE ('#if')
                |UNDERSCORE ('_')
                """.trimMargin()
        )
    }

    @Test
    fun testCharInString() {
        doTest(""""string 'c'" ()""",
                """
                |STRING ('"string 'c'"')
                |WHITESPACE (' ')
                |LPAREN ('(')
                |RPAREN (')')
                """.trimMargin()
        )
    }

    @Test
    fun testCharVerbatimString() {
        doTest("""@"string 'c'" ()""",
                """
                |VERBATIM_STRING ('@"string 'c'"')
                |WHITESPACE (' ')
                |LPAREN ('(')
                |RPAREN (')')
                """.trimMargin()
        )
    }

    @Test
    fun testCharTripleQuoteString() {
        doTest("""""${'"'}string 'c'""${'"'} ()""",
                """
                |TRIPLE_QUOTED_STRING ('""${'"'}string 'c'""${'"'}')
                |WHITESPACE (' ')
                |LPAREN ('(')
                |RPAREN (')')
                """.trimMargin()
        )
    }

    @Test
    fun testLeftArrow() {
        doTest("ident<-ident",
                """
                |IDENT ('ident')
                |LARROW ('<-')
                |IDENT ('ident')
                """.trimMargin()
        )
    }

    @Test
    fun testRightArrow() {
        doTest("t< -> t< ->>",
                """
                |IDENT ('t')
                |LESS ('<')
                |WHITESPACE (' ')
                |RARROW ('->')
                |WHITESPACE (' ')
                |IDENT ('t')
                |LESS ('<')
                |WHITESPACE (' ')
                |PLUS_MINUS_OP ('->>')
                """.trimMargin()
        )
    }

    @Test
    fun testBackslashInString() {
        doTest("""
        |"a\
          b\
		c\

	d"
        |""".trimMargin(),
                """
                |STRING ('"a\\n          b\\n		c\\n\n	d"')
                |NEW_LINE ('\n')
                """.trimMargin()
        )
    }

    @Test
    fun testCommentInTypeApp() {
        doTest("typeof<int//>",
                """
                |IDENT ('typeof')
                |LESS ('<')
                |IDENT ('int')
                |LINE_COMMENT ('//>')
                """.trimMargin()
        )
    }

    @Test
    fun testBlockCommentInTypeApp() {
        doTest("typeof<int(*comment*)> typeof<int<int(*comment*)>>",
                """
                |IDENT ('typeof')
                |LESS ('<')
                |IDENT ('int')
                |BLOCK_COMMENT ('(*comment*)')
                |GREATER ('>')
                |WHITESPACE (' ')
                |IDENT ('typeof')
                |LESS ('<')
                |IDENT ('int')
                |LESS ('<')
                |IDENT ('int')
                |BLOCK_COMMENT ('(*comment*)')
                |GREATER ('>')
                |GREATER ('>')
                """.trimMargin()
        )
    }

    @Test
    fun testValidIdentifiers() {
        doTest("``s<>,.;':\"`~!@#\$%^&*()_+-=``","IDENT ('``s<>,.;':\"`~!@#\$%^&*()_+-=``')")
    }

    @Test
    fun testSmashingGreaterBarRBrack() {
        doTest("let t = [|typeof<string>|]",
                """
                |LET ('let')
                |WHITESPACE (' ')
                |IDENT ('t')
                |WHITESPACE (' ')
                |EQUALS ('=')
                |WHITESPACE (' ')
                |LBRACK_BAR ('[|')
                |IDENT ('typeof')
                |LESS ('<')
                |IDENT ('string')
                |GREATER ('>')
                |BAR_RBRACK ('|]')
                """.trimMargin()
        )
    }

    @Test
    fun testAnonymousRecords() {
        doTest("f<{| C : int |}>x",
                """
                |IDENT ('f')
                |LESS ('<')
                |LBRACE_BAR ('{|')
                |WHITESPACE (' ')
                |IDENT ('C')
                |WHITESPACE (' ')
                |COLON (':')
                |WHITESPACE (' ')
                |IDENT ('int')
                |WHITESPACE (' ')
                |BAR_RBRACE ('|}')
                |GREATER ('>')
                |IDENT ('x')
                """.trimMargin()
        )
    }

    @Test
    fun testAttributeInsideGeneric() {
        doTest("[<MeasureAnnotatedAbbreviation>] type bool<[<Measure>] 'm> = bool",
                """
                |LBRACK_LESS ('[<')
                |IDENT ('MeasureAnnotatedAbbreviation')
                |GREATER_RBRACK ('>]')
                |WHITESPACE (' ')
                |TYPE ('type')
                |WHITESPACE (' ')
                |IDENT ('bool')
                |LESS ('<')
                |LBRACK_LESS ('[<')
                |IDENT ('Measure')
                |GREATER ('>')
                |RBRACK (']')
                |WHITESPACE (' ')
                |QUOTE (''')
                |IDENT ('m')
                |GREATER ('>')
                |WHITESPACE (' ')
                |EQUALS ('=')
                |WHITESPACE (' ')
                |IDENT ('bool')
                """.trimMargin()
        )
    }
}
