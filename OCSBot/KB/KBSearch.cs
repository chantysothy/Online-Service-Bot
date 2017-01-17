using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCSBot.KB
{
    public class KBSearch
    {
        public static KBSearchResult [] Search(string query, string [] subjects = null)
        {
            var threshold = Configuration.ConfigurationHelper.GetFloat("KBSearchScoreThreshold");
            List<KBSearchResult> results = new List<KBSearchResult>();
            var AzureSearchReplcer = new string[] { "如何", "怎麼", "怎麼樣", "怎樣", "該怎樣", "該如何", "要怎樣", "要如何", "的" };
            //search QnAMaker first, then AzSearch
            QnAMakerResult qnaResult = null;
            try
            {
                qnaResult = QnAMakerHelper.SearchKB(query);
            }
            catch (Exception exp)
            {
                //TODO:Log
            }

            if (qnaResult != null && qnaResult.Score > threshold)
            {
                results.Add(new KBSearchResult
                {
                    content = qnaResult.Answer,
                    title = query
                });

                return results.ToArray();
            }
            else
            {
                var newQuery = string.Join(" ", subjects) + " " + query;
                var azsResults = AzSearchHelper.SearchKB(newQuery);
                var azs = azsResults.value?.Where(r => r.Score >= threshold).Select(t => new KBSearchResult()
                {
                    content = t.Answer,
                    title= query,
                    id = t.Id,
                    userfeedback = t.Score
                });
                if (azs != null)
                {
                    return azs.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }
    }
}