using System.Reflection;

using PT.Common;
using PT.Transmissions;

using Xunit.Abstractions;

namespace IPTSerializableUnitTests
{
    public class IPTSerializableTest(ITestOutputHelper a_outputHelper)
    {
        [Fact]
        public void CheckAllUniqueIdsAreUnique()
        {
            //TODO: Grab all the assemblies of the solution instead of just these two projects.
            // We need to ensure uniqueness of all the IPTSerializable UniqueIds, not just the transmissions
            Assembly transmissionsAssembly = Assembly.Load("PT.Transmissions");
            Assembly transmissions2Assembly = Assembly.Load("PT.Transmissions2");
            IEnumerable<Type> transmissionsClasses = transmissionsAssembly.GetExportedTypes().Where(t => t is { IsClass: true, IsAbstract: false } );
            IEnumerable<Type> transmissions2Classes = transmissions2Assembly.GetExportedTypes().Where(t => t is { IsClass: true, IsAbstract: false });

            HashSet<int> transmissionIdsSeen = new HashSet<int>();
            List<(string ClassName, int Id)> duplicates = new List<(string ClassName, int Id)>();
            int highestUniqueIdSeen = -1;
            int exceptionsDiscardedCount = GetUniqueIds(transmissionsClasses, transmissionIdsSeen, duplicates, ref highestUniqueIdSeen);
            exceptionsDiscardedCount += GetUniqueIds(transmissions2Classes, transmissionIdsSeen, duplicates, ref highestUniqueIdSeen);

            string errorMessage = duplicates.Any()
                ? $"Duplicate UniqueIds found: {string.Join(", ", duplicates.Select(dupe => $"{dupe.ClassName}:{dupe.Id}"))}.\n" +
                  "If your transmission's UNIQUE_ID is not the value shown above, make sure you're overriding UniqueId."
                : string.Empty;

            for (int i = 1; i < highestUniqueIdSeen; i++)
            {
                if (!transmissionIdsSeen.Contains(i))
                {
                    a_outputHelper.WriteLine($"An available value for UniqueId is {i}");
                    break;
                }
            }
            
            Assert.True(!duplicates.Any(), errorMessage);
            Assert.True(exceptionsDiscardedCount == 0, "Exceptions were encountered. Make sure all transmissions have parameter-less constructors.");
        }

        /// <summary>
        /// This function iterates through the collection of transmission types passed into it, tries to instantiate
        /// an instance of each type, then grabs the UniqueId value and adds it to the int collections passed in. 
        /// </summary>
        /// <param name="a_pTSerializableClasses">The collection of transmission types to iterate through</param>
        /// <param name="a_transmissionIdsSeen"></param>
        /// <param name="a_duplicates"></param>
        /// <param name="r_highestUniqueIdSeen"></param>
        /// <returns>How many exceptions encountered while going through the transmission types</returns>
        private int GetUniqueIds(IEnumerable<Type> a_pTSerializableClasses, HashSet<int> a_transmissionIdsSeen, IList<(string ClassName, int Id)> a_duplicates, ref int r_highestUniqueIdSeen)
        {
            int exceptionsDiscardedCount = 0;
            
            foreach (Type ptSerializable in a_pTSerializableClasses)
            {
                if (!typeof(IPTSerializable).IsAssignableFrom(ptSerializable))
                {
                    // There are classes that derive from PTObjectBaseEdit that we don't care about in the projects too
                    continue; 
                }

                try
                {
                    var transmissionInstance = Activator.CreateInstance(ptSerializable);
                    int uniqueId = (int)ptSerializable.GetProperty("UniqueId").GetValue(transmissionInstance);

                    r_highestUniqueIdSeen = r_highestUniqueIdSeen < uniqueId ? uniqueId : r_highestUniqueIdSeen;
                    if (!a_transmissionIdsSeen.Add(uniqueId))
                    {
                        a_duplicates.Add((ptSerializable.Name, uniqueId));
                    }
                }
                catch (Exception exception)
                {
                    a_outputHelper.WriteLine($"Could not instantiate {ptSerializable.Name}. Please make sure it has a parameter-less constructor.");
                    exceptionsDiscardedCount += 1;
                }
            }

            return exceptionsDiscardedCount;
        }
    }
}