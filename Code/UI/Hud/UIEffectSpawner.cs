using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace UI.Resources
{
    public class UIEffectSpawner : MonoBehaviour
    {
        [SerializeField] private UIEffectView _effectPrefab;

        //private WindowsManager windowsManager;
        //private GenericFactory<ResourceView> resourceViewFactory;
        //private ResourceDropperConfig resourceDropperConfig;
        private Canvas _canvas;
        private List<UIEffectView> poolResourceSprites = new();
        private CustomEvent dropEvent;

        [Inject]
        public void Construct()
        {
            //this.resourceViewFactory = resourceViewFactory;
            //this.windowsManager = windowsManager;
            //this.resourceDropperConfig = resourceDropperConfig;
        }
        
        private void Start()
        {
            _canvas = GetComponentInParent<Canvas>();
            dropEvent = OnEvent;
            Translator.Add<HUDProtocol>(dropEvent);
        }

        private void OnDestroy()
        {
            Translator.Remove<HUDProtocol>(dropEvent);
        }

        private void SpawnUIEffect(bool isCrit, string damage)
        {
            GetImage().Animate(isCrit, damage);
        }

        private UIEffectView GetImage()
        {
            foreach (UIEffectView image in poolResourceSprites)
            {
                if (!image.gameObject.activeSelf)
                {
                    image.gameObject.SetActive(true);
                    return image;
                }
            }
            UIEffectView rectTransform = CreateNewImage();
            poolResourceSprites.Add(rectTransform);
            return rectTransform;
        }

        private UIEffectView CreateNewImage()
        {
            UIEffectView effect = Instantiate(_effectPrefab);
            effect.transform.SetParent(transform);
            effect.transform.localScale = Vector3.one;
            effect.Init();
            return effect;
        }

        private void OnEvent(Enum code, ISendData data)
        {
            switch (code)
            {
                case HUDProtocol.CreateUIEffect:
                    var effectData = (DamageEffectData)data;
                    SpawnUIEffect(effectData.isCrit, effectData.damage);
                    break;
            }
        }
    }
}