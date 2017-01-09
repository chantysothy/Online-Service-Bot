using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCSBot.KB
{
    public class KBSearch
    {
        public static KBSearchResult [] Search(string query)
        {
            var threshold = Configuration.ConfigurationHelper.GetFloat("KBSearchScoreThreshold");
            List<KBSearchResult> results = new List<KBSearchResult>();

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
                var azsResults = AzSearchHelper.SearchKB(query);
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