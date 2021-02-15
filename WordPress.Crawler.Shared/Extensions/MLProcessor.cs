using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using WordPress.Crawler.Shared.Models;

namespace WordPress.Crawler.Shared.Extensions
{
    public static class MLProcessor
    {
        public static List<WordBagResult> BagOfWords(List<Input> inputData)
        {
            var context = new MLContext();
            var dataView = context.Data.LoadFromEnumerable(inputData);
            var bagOfWordPipeline = context.Transforms.Text.ProduceWordBags(
                "BagOfWords",
                "Text",
                ngramLength: 1,
                useAllLengths: false,
                weighting: NgramExtractingEstimator.WeightingCriteria.TfIdf);
            var bagOfWordTransform = bagOfWordPipeline.Fit(dataView);
            var bagOfWordsDataView = bagOfWordTransform.Transform(dataView);
            var predictionEngine = context.Model.CreatePredictionEngine<Input, WordBagOutput>(bagOfWordTransform);
            var prediction = predictionEngine.Predict(inputData[0]);

            VBuffer<ReadOnlyMemory<char>> slotNames = default;
            bagOfWordsDataView.Schema["BagOfWords"].GetSlotNames(ref slotNames);
            var bagOfWordsColumn = bagOfWordsDataView.GetColumn<VBuffer<float>>(bagOfWordsDataView.Schema["BagOfWords"]);
            var slots = slotNames.GetValues();

            //int length = bagOfWordsColumn.Count();
            var result = new List<WordBagResult>();

            foreach (var row in bagOfWordsColumn)
            {
                foreach (var item in row.Items())
                {
                    result.Add(new WordBagResult
                    {
                        Word = $"{slots[item.Key]}"
                    }) ;
                }
            }

            for (int i = 0; i < result.Count(); i++)
            {
                result[i].Count = prediction.BagOfWords[i];
            }

            return result;
        }
    }
}
