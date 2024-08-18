//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class LocaliztionTextList : LocObject
//{
//    [SerializeField] private StringField t_UIElement;
//    public int indexKey;
//    List<string> keys = new List<string>();
//    private int maxValue;

//    public StringField TUIElement
//    {
//        get
//        {
//            return t_UIElement;
//        }

//        set
//        {
//            t_UIElement = value;
//        }
//    }

//    private void OnEnable()
//    {
//        SetNewKeys();

//        LanguageSettings.language += OnSetLanguage;

//        OnSetLanguage();
//    }

//    private void OnDisable()
//    {
//        LanguageSettings.language -= OnSetLanguage;
//    }

//    private void SetNewKeys()
//    {
//        keys = new List<string>();

//        keys.AddRange(t_UIElement.value.Split(new char[] { ':' }));
//        maxValue = keys.Count;
//        maxValue--;
//    }

//    public override void OnSetLanguage()
//    {
//        SetNewKeys();
//        SetLanguage(gameObject.GetComponent<Text>(), t_UIElement.value);
//    }

//    public Text SetLanguage(Text to, string key)
//    {
//        to.text = keys[indexKey];

//        return to;
//    }

//    public void NextText()
//    {
//        SetNewKeys();
//        if (indexKey < maxValue)
//        {
//            indexKey++;
//        }
//        else
//        {
//            indexKey = 0;
//        }

//        gameObject.GetComponent<Text>().text = keys[indexKey];
//    }

//    public void BackText()
//    {
//        SetNewKeys();
//        if (indexKey > 0)
//        {
//            indexKey--;
//        }
//        else
//        {
//            indexKey = maxValue;
//        }

//        gameObject.GetComponent<Text>().text = keys[indexKey];
//    }
//}