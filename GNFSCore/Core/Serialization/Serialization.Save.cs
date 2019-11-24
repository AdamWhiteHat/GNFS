using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using ExtendedArithmetic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace GNFSCore
{

    using Factors;
    using Interfaces;

    public static partial class Serialization
    {
        public static class Save
        {
            public static void Object(object obj, string filename)
            {
                string saveJson = JsonConvert.SerializeObject(obj, Formatting.Indented);
                File.WriteAllText(filename, saveJson);
            }

            public static void All(GNFS gnfs)
            {
                Save.Gnfs(gnfs);

                int counter = 1;
                foreach (Polynomial poly in gnfs.PolynomialCollection)
                {
                    string filename = $"Polynomial.{counter:00}";
                    Save.Object(poly, Path.Combine(gnfs.SaveLocations.SaveDirectory, filename));
                    counter++;
                }

                //Save.FactorBase.Rational(gnfs);
                //Save.FactorBase.Algebraic(gnfs);
                //Save.FactorBase.Quadratic(gnfs);

                Save.FactorPair.Rational(gnfs);
                Save.FactorPair.Algebraic(gnfs);
                Save.FactorPair.Quadratic(gnfs);

                Save.Relations.Smooth.Append(gnfs);
                Save.Relations.Rough.Append(gnfs);
                Save.Relations.Free.AllSolutions(gnfs);
            }

            public static void Gnfs(GNFS gnfs)
            {
                Save.Object(gnfs, gnfs.SaveLocations.GnfsParameters_SaveFile);
            }

            /*
			public static class FactorBase
			{
				public static void Rational(GNFS gnfs)
				{
					if (gnfs.PrimeFactorBase.RationalFactorBase.Any())
					{
						Save.Object(gnfs.PrimeFactorBase.RationalFactorBase, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFSCore.FactorBase.RationalFactorBase)}.json"));
					}
				}

				public static void Algebraic(GNFS gnfs)
				{
					if (gnfs.PrimeFactorBase.AlgebraicFactorBase.Any())
					{
						Save.Object(gnfs.PrimeFactorBase.AlgebraicFactorBase, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFSCore.FactorBase.AlgebraicFactorBase)}.json"));
					}
				}

				public static void Quadratic(GNFS gnfs)
				{
					if (gnfs.PrimeFactorBase.QuadraticFactorBase.Any())
					{
						Save.Object(gnfs.PrimeFactorBase.QuadraticFactorBase, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFSCore.FactorBase.QuadraticFactorBase)}.json"));
					}
				}
			}
			*/

            public static class FactorPair
            {
                public static void Rational(GNFS gnfs)
                {
                    if (gnfs.RationalFactorPairCollection.Any())
                    {
                        Save.Object(gnfs.RationalFactorPairCollection, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFS.RationalFactorPairCollection)}.json"));
                    }
                }

                public static void Algebraic(GNFS gnfs)
                {
                    if (gnfs.AlgebraicFactorPairCollection.Any())
                    {
                        Save.Object(gnfs.AlgebraicFactorPairCollection, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFS.AlgebraicFactorPairCollection)}.json"));
                    }
                }

                public static void Quadratic(GNFS gnfs)
                {
                    if (gnfs.QuadraticFactorPairCollection.Any())
                    {
                        Save.Object(gnfs.QuadraticFactorPairCollection, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFS.QuadraticFactorPairCollection)}.json"));
                    }
                }
            }

            public static class Relations
            {
                public static class Smooth
                {
                    private static bool? _fileExists = null;
                    private static bool FileExists(GNFS gnfs)
                    {
                        if (!_fileExists.HasValue || _fileExists == false)
                        {
                            _fileExists = File.Exists(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.SmoothRelations)}.json"));
                        }
                        return _fileExists.Value;
                    }

                    public static void Append(GNFS gnfs)
                    {
                        if (gnfs.CurrentRelationsProgress.Relations.SmoothRelations.Any())
                        {
                            List<Relation> toSave = gnfs.CurrentRelationsProgress.Relations.SmoothRelations.Where(rel => !rel.IsPersisted).ToList();
                            foreach (Relation rel in toSave)
                            {
                                Append(gnfs, rel);
                            }
                        }
                    }

                    public static void Append(GNFS gnfs, Relation relation)
                    {
                        if (relation != null && relation.IsSmooth && !relation.IsPersisted)
                        {
                            string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.SmoothRelations)}.json");
                            string json = JsonConvert.SerializeObject(relation, Formatting.Indented);

                            if (FileExists(gnfs))
                            {
                                json = json.Insert(0, ",");
                            }

                            File.AppendAllText(filename, json);

                            gnfs.CurrentRelationsProgress.SmoothRelationsCounter += 1;

                            relation.IsPersisted = true;
                        }
                    }
                }

                public static class Rough
                {
                    private static bool? _fileExists = null;
                    private static bool FileExists(GNFS gnfs)
                    {
                        if (!_fileExists.HasValue || _fileExists == false)
                        {
                            _fileExists = File.Exists(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.RoughRelations)}.json"));
                        }
                        return _fileExists.Value;
                    }

                    public static void Append(GNFS gnfs)
                    {
                        if (gnfs.CurrentRelationsProgress.Relations.RoughRelations.Any())
                        {
                            List<Relation> toSave = gnfs.CurrentRelationsProgress.Relations.RoughRelations.Where(rel => !rel.IsPersisted).ToList();
                            foreach (Relation rel in toSave)
                            {
                                Append(gnfs, rel);
                            }
                        }
                    }

                    public static void Append(GNFS gnfs, Relation roughRelation)
                    {
                        if (roughRelation != null && !roughRelation.IsSmooth && !roughRelation.IsPersisted)
                        {
                            string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.RoughRelations)}.json");
                            string json = JsonConvert.SerializeObject(roughRelation, Formatting.Indented);

                            if (FileExists(gnfs))
                            {
                                json += ",";
                            }

                            File.AppendAllText(filename, json);
                            roughRelation.IsPersisted = true;
                        }
                    }

                    public static void AppendList(GNFS gnfs, List<Relation> roughRelations)
                    {
                        if (roughRelations != null && roughRelations.Any())
                        {
                            string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.RoughRelations)}.json");
                            string json = JsonConvert.SerializeObject(roughRelations, Formatting.Indented);
                            json = json.Replace("[", "").Replace("]", ",");
                            File.AppendAllText(filename, json);
                            roughRelations.ForEach(rel => rel.IsPersisted = true);
                        }
                    }
                }

                public static class Free
                {
                    public static void AllSolutions(GNFS gnfs)
                    {
                        if (gnfs.CurrentRelationsProgress.Relations.FreeRelations.Any())
                        {
                            gnfs.CurrentRelationsProgress.FreeRelationsCounter = 1;
                            foreach (List<Relation> solution in gnfs.CurrentRelationsProgress.Relations.FreeRelations)
                            {
                                SingleSolution(gnfs, solution);
                            }
                        }
                    }

                    public static void SingleSolution(GNFS gnfs, List<Relation> solution)
                    {
                        if (solution.Any())
                        {
                            solution.ForEach(rel => rel.IsPersisted = true);
                            Save.Object(solution, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.FreeRelations)}_{gnfs.CurrentRelationsProgress.FreeRelationsCounter}.json"));
                            gnfs.CurrentRelationsProgress.FreeRelationsCounter += 1;
                        }
                    }
                }
            }
        }
    }
}
