using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchImport
{
    [SerializePropertyNamesAsCamelCase]
    public class SearchItem
    {
        [Key]
        [IsSearchable,IsRetrievable(true)]
        public string Id { get; set; }
        [IsSearchable, IsSortable, IsFilterable, IsRetrievable(true), IsFacetable]
        public string query { get; set; }
        [IsSearchable, IsSortable, IsFilterable, IsRetrievable(true), IsFacetable]
        public string answer { get; set; }
        [IsSearchable, IsSortable, IsFilterable, IsRetrievable(true), IsFacetable]
        public string category { get; set; }
        [IsSearchable, IsSortable, IsFilterable, IsRetrievable(true), IsFacetable]
        public string subcategory { get; set; }
    }
}
