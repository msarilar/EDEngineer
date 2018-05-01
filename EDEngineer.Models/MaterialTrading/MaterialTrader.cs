using System;
using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models.State;
using EDEngineer.Models.Utils;

namespace EDEngineer.Models
{
    public static class MaterialTrader
    {
        public static IEnumerable<MaterialTrade> FindPossibleTrades(StateCargo cargo, Dictionary<Entry, int> missingIngredients, Dictionary<EntryData, int> deduced)
        {
            var ingredients = cargo.Ingredients.Values
                                   .Where(i => i.Count > 0 && 
                                               i.Data.Group.HasValue && 
                                               i.Data.Rarity.Rank() != null &&
                                               !missingIngredients.ContainsKey(i)).ToList();

            var allTrades = AllTrades(missingIngredients, ingredients, deduced).ToList();

            var coveredTrades = allTrades.Where(trade => trade.WillBeEnough).OrderBy(trade => trade.TradedNeeded).ToList();
            var incompleteTrades = allTrades.Where(trade => !trade.WillBeEnough).OrderBy(trade => trade.TradedNeeded).ToList();

            var coveredIngredients = new HashSet<EntryData>();
            foreach (var currentTrade in coveredTrades)
            {
                if (currentTrade.Traded.Count - currentTrade.TradedNeeded - deduced.GetOrDefault(currentTrade.Traded.Data) < 0)
                {
                    continue;
                }

                coveredIngredients.Add(currentTrade.Needed.Data);

                if (deduced.ContainsKey(currentTrade.Traded.Data))
                {
                    deduced[currentTrade.Traded.Data] += currentTrade.TradedNeeded;
                }
                else
                {
                    deduced[currentTrade.Traded.Data] = currentTrade.TradedNeeded;
                }

                yield return currentTrade;
            }

            foreach (var currentTrade in incompleteTrades)
            {
                if (coveredIngredients.Contains(currentTrade.Needed.Data))
                {
                    continue;
                }

                if (deduced.ContainsKey(currentTrade.Traded.Data))
                {
                    deduced[currentTrade.Traded.Data] += currentTrade.TradedNeeded;
                }
                else
                {
                    deduced[currentTrade.Traded.Data] = currentTrade.TradedNeeded;
                }

                yield return currentTrade;
            }
        }

        private static IEnumerable<MaterialTrade> AllTrades(Dictionary<Entry, int> missingIngredients,
                                                            List<Entry> ingredients, Dictionary<EntryData, int> deduced)
        {
            foreach (var missingIngredient in missingIngredients)
            {
                var ingredient = missingIngredient.Key;
                var missingSize = missingIngredient.Value;

                if (missingSize <= 0 ||
                    !ingredient.Data.Group.HasValue ||
                    ingredient.Data.Group.In(Group.Thargoid, Group.Guardian, Group.Commodities) ||
                    !ingredient.Data.Rarity.Rank().HasValue)
                {
                    continue;
                }

                var group = ingredient.Data.Group.Value;
                // ReSharper disable PossibleInvalidOperationException
                var targetRank = ingredient.Data.Rarity.Rank().Value;

                // find same group ingredients
                var sameGroupIngredients = ingredients.Where(i => i.Data.Group == group).ToList();
                foreach (var sameGroup in sameGroupIngredients)
                {
                    var sourceRank = sameGroup.Data.Rarity.Rank().Value;
                    var rankDifference = sourceRank - targetRank;
                    int needed;
                    if (rankDifference > 0)
                    {
                        needed = (int) Math.Ceiling(missingSize / Math.Pow(3, rankDifference));
                    }
                    else
                    {
                        needed = (int) (Math.Pow(6, Math.Abs(rankDifference)) * missingSize);
                    }

                    var willBeEnough = needed + deduced.GetOrDefault(sameGroup.Data) <= sameGroup.Count;
                    yield return new MaterialTrade(sameGroup, ingredient, needed, missingSize, willBeEnough, deduced.GetOrDefault(sameGroup.Data));
                }

                // find other group ingredients
                var differentGroupIngredients = ingredients.Where(i => i.Data.Group != group && i.Data.Kind == ingredient.Data.Kind && i.Data.Subkind == ingredient.Data.Subkind).ToList();
                foreach (var otherGroup in differentGroupIngredients)
                {
                    var sourceRank = otherGroup.Data.Rarity.Rank().Value;
                    var rankDifference = sourceRank - targetRank;
                    int needed;
                    if (rankDifference > 0)
                    {
                        needed = 2 * (int) Math.Ceiling(missingSize / Math.Pow(3, rankDifference - 1));
                    }
                    else if (rankDifference == 0)
                    {
                        needed = 6 * missingSize;
                    }
                    else
                    {
                        continue;
                    }

                    var willBeEnough = needed + deduced.GetOrDefault(otherGroup.Data) <= otherGroup.Count;

                    yield return new MaterialTrade(otherGroup, ingredient, needed, missingSize, willBeEnough, deduced.GetOrDefault(otherGroup.Data));
                }
                // ReSharper restore PossibleInvalidOperationException
            }
        }
    }
}