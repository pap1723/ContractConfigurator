﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using ContractConfigurator.Util;

namespace ContractConfigurator.ExpressionParser
{
    /// <summary>
    /// Expression parser subclass for Biome.
    /// </summary>
    public class BiomeParser : ClassExpressionParser<Biome>, IExpressionParserRegistrer
    {
        static BiomeParser()
        {
            RegisterMethods();
        }

        public void RegisterExpressionParsers()
        {
            RegisterParserType(typeof(Biome), typeof(BiomeParser));
        }

        internal static void RegisterMethods()
        {
            RegisterMethod(new Method<Biome, CelestialBody>("CelestialBody", biome => biome == null ? null : biome.body));
            RegisterMethod(new Method<Biome, bool>("IsKSC", biome => biome == null ? false : biome.IsKSC()));
            RegisterMethod(new Method<Biome, float>("RemainingScience", RemainingScience));

            RegisterMethod(new Method<Biome, List<Location>>("DifficultLocations", biome => biome == null ?
                new List<Location>() : BiomeTracker.GetDifficultLocations(biome.body, biome.biome).Select(v => new Location(v.y, v.x)).ToList()));

            RegisterGlobalFunction(new Function<List<Biome>>("KSCBiomes", () => Biome.KSCBiomes.Select(b =>
                new Biome(FlightGlobals.Bodies.Where(cb => cb.isHomeWorld).Single(), b)).ToList(), false));
        }

        public BiomeParser()
        {
        }

        private static float RemainingScience(Biome biome)
        {
            if (biome == null || HighLogic.CurrentGame == null)
            {
                return 0.0f;
            }

            return Science.GetSubjects(new CelestialBody[] { biome.body }, null, b => b == biome.biome).Sum(subj =>
                subj.scienceCap * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier - subj.science);
        }

        internal override Biome ParseIdentifier(Token token)
        {
            // Try to parse more, as biome names can have spaces
            Match m = Regex.Match(expression, @"^((?>\s*[A-Za-z][\w\d]*)+).*");
            string identifier = m.Groups[1].Value;
            expression = (expression.Length > identifier.Length ? expression.Substring(identifier.Length) : "");
            identifier = token.sval + identifier;

            // Special case for null
            if (identifier == "null")
            {
                return null;
            }

            return new Biome(null, identifier);
        }
    }
}