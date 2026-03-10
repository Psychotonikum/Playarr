using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Annotations;

namespace Playarr.Core.Indexers.Newznab
{
    public static class NewznabCategoryFieldOptionsConverter
    {
        public static List<FieldSelectOption<int>> GetFieldSelectOptions(List<NewznabCategory> categories)
        {
            // Categories not relevant for Playarr (game ROM manager)
            var ignoreCategories = new[] { 4000, 6000, 7000 };

            // TV and other categories less relevant for games
            var unimportantCategories = new[] { 0, 2000, 3000, 5000 };

            var result = new List<FieldSelectOption<int>>();

            if (categories == null)
            {
                // Fetching categories failed, use default Console categories
                categories = new List<NewznabCategory>();
                categories.Add(new NewznabCategory
                {
                    Id = 1000,
                    Name = "Console",
                    Subcategories = new List<NewznabCategory>
                    {
                        new NewznabCategory { Id = 1010, Name = "NDS" },
                        new NewznabCategory { Id = 1020, Name = "PSP" },
                        new NewznabCategory { Id = 1030, Name = "Wii" },
                        new NewznabCategory { Id = 1040, Name = "Xbox" },
                        new NewznabCategory { Id = 1050, Name = "Xbox 360" },
                        new NewznabCategory { Id = 1060, Name = "Wii U" },
                        new NewznabCategory { Id = 1070, Name = "PS3" },
                        new NewznabCategory { Id = 1080, Name = "Other" },
                        new NewznabCategory { Id = 1090, Name = "3DS" },
                        new NewznabCategory { Id = 1100, Name = "PS Vita" },
                        new NewznabCategory { Id = 1110, Name = "Xbox One" },
                        new NewznabCategory { Id = 1120, Name = "PS4" },
                        new NewznabCategory { Id = 1130, Name = "Switch" },
                        new NewznabCategory { Id = 1140, Name = "PS5" },
                        new NewznabCategory { Id = 1150, Name = "Xbox Series X" },
                        new NewznabCategory { Id = 1180, Name = "PC" },
                    }
                });
            }

            foreach (var category in categories.Where(cat => !ignoreCategories.Contains(cat.Id)).OrderBy(cat => unimportantCategories.Contains(cat.Id)).ThenBy(cat => cat.Id))
            {
                result.Add(new FieldSelectOption<int>
                {
                    Value = category.Id,
                    Name = category.Name,
                    Hint = $"({category.Id})"
                });

                if (category.Subcategories != null)
                {
                    foreach (var subcat in category.Subcategories.OrderBy(cat => cat.Id))
                    {
                        result.Add(new FieldSelectOption<int>
                        {
                            Value = subcat.Id,
                            Name = subcat.Name,
                            Hint = $"({subcat.Id})",
                            ParentValue = category.Id
                        });
                    }
                }
            }

            return result;
        }
    }
}
