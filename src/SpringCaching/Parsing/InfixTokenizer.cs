﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Parsing
{
    /// <summary>
    /// A tokenizer that parses an input expression string in infix notation into tokens without white spaces.
    /// https://github.com/soukoku/ExpressionParser
    /// </summary>
    public class InfixTokenizer //: IExpressionTokenizer
    {
        private List<ExpressionToken>? _currentTokens;

        /// <summary>
        /// Splits the specified input into a list of <see cref="ExpressionToken" /> values.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public ExpressionToken[] Tokenize(string input)
        {
            _currentTokens = new List<ExpressionToken>();
            ExpressionToken? lastExpToken = null;

            var reader = new ListReader<RawToken>(new RawTokenizer().Tokenize(input));

            while (!reader.IsEnd)
            {
                var curRawToken = reader.Read();
                switch (curRawToken.TokenType)
                {
                    case RawTokenType.WhiteSpace:
                        // generially ends previous token outside other special scopes
                        lastExpToken = null;
                        break;
                    case RawTokenType.Literal:
                        if (lastExpToken == null || lastExpToken.TokenType != ExpressionTokenType.Value)
                        {
                            lastExpToken = new ExpressionToken { TokenType = ExpressionTokenType.Value };
                            _currentTokens.Add(lastExpToken);
                        }
                        lastExpToken.Append(curRawToken);
                        break;
                    case RawTokenType.Symbol:
                        // first do operator match by checking the prev op
                        // and see if combined with current token would still match a known operator
                        if (KnownOperators.IsKnown(curRawToken.Value))
                        {
                            if (lastExpToken != null && lastExpToken.TokenType == ExpressionTokenType.Operator)
                            {
                                var testOpValue = lastExpToken.Value + curRawToken.Value;
                                if (KnownOperators.IsKnown(testOpValue))
                                {
                                    // just append it
                                    lastExpToken.Append(curRawToken);
                                    continue;
                                }
                            }
                            // start new one
                            lastExpToken = new ExpressionToken { TokenType = ExpressionTokenType.Operator };
                            _currentTokens.Add(lastExpToken);
                            lastExpToken.Append(curRawToken);
                        }
                        else
                        {
                            lastExpToken = HandleNonOperatorSymbolToken(reader, lastExpToken, curRawToken);
                        }
                        break;
                    default:
                        // should never happen
                        throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Unsupported token type {0} at position {1}.", curRawToken.TokenType, curRawToken.Position));
                }
            }

            MassageTokens(_currentTokens);

            return _currentTokens.ToArray();
        }

        private ExpressionToken? HandleNonOperatorSymbolToken(ListReader<RawToken> reader, ExpressionToken? lastExpToken, RawToken curRawToken)
        {
            switch (curRawToken.Value)
            {
                case ",":
                    lastExpToken = new ExpressionToken { TokenType = ExpressionTokenType.Comma };
                    _currentTokens!.Add(lastExpToken);
                    lastExpToken.Append(curRawToken);
                    break;
                case "(":
                    // if last one is string make it a function
                    if (lastExpToken != null && lastExpToken.TokenType == ExpressionTokenType.Value)
                    {
                        lastExpToken.TokenType = ExpressionTokenType.Function;
                    }

                    lastExpToken = new ExpressionToken { TokenType = ExpressionTokenType.OpenParenthesis };
                    _currentTokens!.Add(lastExpToken);
                    lastExpToken.Append(curRawToken);
                    break;
                case ")":
                    lastExpToken = new ExpressionToken { TokenType = ExpressionTokenType.CloseParenthesis };
                    _currentTokens!.Add(lastExpToken);
                    lastExpToken.Append(curRawToken);
                    break;
                case "{":
                    // read until end of }
                    lastExpToken = ReadToLiteralAs(reader, "}", ExpressionTokenType.Field);
                    break;
                case "\"":
                    // read until end of "
                    lastExpToken = ReadToLiteralAs(reader, "\"", ExpressionTokenType.DoubleQuoted);
                    break;
                case "'":
                    // read until end of '
                    lastExpToken = ReadToLiteralAs(reader, "'", ExpressionTokenType.SingleQuoted);
                    break;
            }

            return lastExpToken;
        }

        private ExpressionToken ReadToLiteralAs(ListReader<RawToken> reader, string literalValue, ExpressionTokenType tokenType)
        {
            var lastExpToken = new ExpressionToken { TokenType = tokenType };
            _currentTokens!.Add(lastExpToken);
            while (!reader.IsEnd)
            {
                var next = reader.Read();
                if (next.TokenType == RawTokenType.Symbol && next.Value == literalValue)
                {
                    break;
                }
                lastExpToken.Append(next);
            }

            return lastExpToken;
        }

        private static void MassageTokens(List<ExpressionToken> tokens)
        {
            // do final token parsing based on contexts and cleanup

            var reader = new ListReader<ExpressionToken>(tokens);
            while (!reader.IsEnd)
            {
                var tk = reader.Read();

                if (tk.TokenType == ExpressionTokenType.Operator)
                {
                    // special detection for operators depending on where it is :(
                    DetermineOperatorType(reader, tk);
                }

                tk.Freeze();
            }
        }

        private static void DetermineOperatorType(ListReader<ExpressionToken> reader, ExpressionToken tk)
        {
            tk.OperatorType = KnownOperators.TryMap(tk.Value);
            switch (tk.OperatorType)
            {
                case OperatorType.PreDecrement:
                case OperatorType.PreIncrement:
                    // detect if it's really post ++ -- versions 
                    var prev = reader.Position > 1 ? reader.Peek(-2) : null;
                    if (prev != null && prev.TokenType == ExpressionTokenType.Value)
                    {
                        if (tk.OperatorType == OperatorType.PreIncrement)
                        {
                            tk.OperatorType = OperatorType.PostIncrement;
                        }
                        else
                        {
                            tk.OperatorType = OperatorType.PostDecrement;
                        }
                    }
                    break;
                case OperatorType.Addition:
                case OperatorType.Subtraction:
                    // detect if unary + -
                    prev = reader.Position > 1 ? reader.Peek(-2) : null;
                    if (prev == null ||
                        (prev.TokenType == ExpressionTokenType.Operator &&
                        prev.OperatorType != OperatorType.PostDecrement &&
                        prev.OperatorType != OperatorType.PostIncrement))
                    {
                        if (tk.OperatorType == OperatorType.Addition)
                        {
                            tk.OperatorType = OperatorType.UnaryPlus;
                        }
                        else
                        {
                            tk.OperatorType = OperatorType.UnaryMinus;
                        }
                    }
                    break;
                case OperatorType.None:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Operator {0} is not supported.", tk.Value));
            }
        }
    }
}
