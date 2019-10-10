using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Money
{
    public class ProductView : MonoBehaviour
    {
        private Product _product;

        public Text gemsText;

        public Text priceText;

        public Button buyButton;

        public Text buyButtonLabel;

        public void Initialize(Product product)
        {
            _product = product;
            gemsText.text = gemsText.text.Replace("{gems_count}", product.CurrencyCount.ToString()) ;
            priceText.text = priceText.text.Replace("{price}", product.Price.ToString());

            if (_product.BoughtLimit != null)
            {
                gemsText.text += " (" + _product.BoughtCount + "/" + _product.BoughtLimit + ")";
                if (_product.BoughtCount == _product.BoughtLimit)
                {
                    buyButtonLabel.text = "Sold";
                }
            }
        }
        
        public bool Sold => _product.BoughtLimit != null && _product.BoughtCount == _product.BoughtLimit;
    }
}