using System;

using org.mariuszgromada.math.mxparser;

using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Devices.Power;

namespace EE_Calculator.MathEngine {

    public class PageEngine
    {
        private List<Expression> _expressions;
        private Dictionary<string, Argument> _arguments;

        // regex pattern for Argument
        private static string arg_patt = @"^\s*(\w+)\s*=(.*)";
        private static Regex argument_regex = new Regex(arg_patt);

        public PageEngine()
        {
            License.iConfirmNonCommercialUse("https://github.com/idanre1/ee_calc");
        }

        // method that split a string by new lines to list of strings
        private static List<string> SplitByNewLine(string s)
        {
            string[] lines = s.Replace("_","") // remove underscores for hex/bin results
                                    .Split(
                                    new string[] { "\r\n", "\r", "\n" },
                                    StringSplitOptions.None
                                    );
            return new List<string>(lines);
        }

        private List<Expression> GetExpressionsFromString(string s)
        {
            List<string> list = SplitByNewLine(s); // also remove _ from hex/bin results

            // get size of list
            _expressions = new List<Expression>(list.Count);

            // convert the list of strings to list of expressions using foreach
            foreach (var item in list)
            {
                _expressions.Add(new Expression(item));
            }
            return _expressions;
        }

        private List<double> CalcExpressions(List<Expression> expressions)
        {
            var cnt = expressions.Count;
            var results = new List<double>(cnt);
            var answers = new List<double>();

            for (int i=0; i < cnt; i++) {
                // Take Expression
                var item = expressions[i];
                // Add arguments to the expression
                foreach (var arg in _arguments)
                {
                    item.addArguments(arg.Value);
                }
                var syntax = item.checkSyntax();

                if (syntax) // Valid expression
                {
                    // Calculate the expression
                    var calc = item.calculate();
                    // Add to results
                    results.Add(calc);
                    // Also add answer to arguments
                    _arguments["e" + i] = new Argument($"e{i} = {calc}");
                }
                else if (i == cnt - 1)
                {
                    // last result:
                    // 1. Argument does not contribute to the result
                    // 2. Ignore inter-typing valus as arguments
                    results.Add(double.NaN);
                } else { // Try to search for an argument
                    
                    var (arg_name, argument) = getArgument(item);
                    if (arg_name is string)
                    {
                        // Valid expression
                        if (argument.checkSyntax())
                        {
                            _arguments[arg_name] = argument;
                        }
                    }
                    results.Add(double.NaN);
                }
            }
            return results;
        }

        private (string,Argument) getArgument(Expression exp)
        {
            var s = exp.getExpressionString();

            // search by regex s to a=b
            var match = argument_regex.Match(s);
            var cnt = match.Groups.Count;
            if (cnt == 3)
            {
                var name = match.Groups[1].ToString();
                var value = match.Groups[2].ToString();
                // argument could also be an Expression
                Argument argument = new Argument($"{name} = {value}");
                foreach (var arg in _arguments)
                {
                    argument.addArguments(arg.Value);
                }

                return (name, argument);
            }
            return (null, null);
        }

        private List<string> ConvertResults(List<double> results)
        {
            var cnt = results.Count;
            var answers = new List<string>(results.Count);

            for (int i = 0; i < cnt; i++)
            {
                var item = results[i];
                if (double.IsNaN(item))
                {
                    answers.Add("");
                }
                else
                {
                    answers.Add($"e{i}");
                }
            }
            return answers;
        }

        public (string,string,string,string) Calc(string s)
        {
            // Pre-Process inputs
            var expressions = GetExpressionsFromString(s);
            // Handle inits
            _arguments = new Dictionary<string, Argument>();

            // Process
            var results = CalcExpressions(expressions);
            var answers = ConvertResults(results);

            // print to UI
            var dbl = string.Join(Environment.NewLine, results);
            var hx = string.Join(Environment.NewLine,
                results.Select(x => MathFormatter.FormatHex(x))
                );
            var bn = string.Join(Environment.NewLine,
                results.Select(x => MathFormatter.FormatBin(x))
                );
            var ans = string.Join(Environment.NewLine, answers);

            return (dbl, hx, bn, ans);

        }
    }
}
