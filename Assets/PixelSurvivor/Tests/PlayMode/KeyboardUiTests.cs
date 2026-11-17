using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PixelSurvivor.Tests
{
    public sealed class KeyboardUiTests
    {
        [UnityTest]
        public IEnumerator WasdAndEnterNavigateCampaignMenu()
        {
            yield return SceneManager.LoadSceneAsync("Assets/PixelSurvivor/Scenes/MainScene.unity");
            yield return null;

            var game=Object.FindAnyObjectByType<GameManager>();
            Assert.NotNull(game);
            game.Campaign.UseTemporaryProgress();
            var events=EventSystem.current;
            var input=events.GetComponent<InputSystemUIInputModule>();
            Assert.NotNull(input);
            Assert.IsFalse(input.deselectOnBackgroundClick);
            Assert.AreEqual("시작 Button",events.currentSelectedGameObject.name);
            Assert.IsTrue(events.currentSelectedGameObject.GetComponent<Outline>().enabled);

            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.S));
                yield return null;
                Assert.AreEqual("영구 강화 Button",events.currentSelectedGameObject.name);

                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));
                yield return null;
                Assert.AreEqual("시작 Button",events.currentSelectedGameObject.name);

                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.DownArrow));
                yield return null;
                Assert.AreEqual("영구 강화 Button",events.currentSelectedGameObject.name);

                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.UpArrow));
                yield return null;
                Assert.AreEqual("시작 Button",events.currentSelectedGameObject.name);

                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter));
                yield return null;
                Assert.AreEqual("Character 0 Button",events.currentSelectedGameObject.name);
                var previous=events.currentSelectedGameObject;
                var hovered=GameObject.Find("Character 4 Button");
                Assert.NotNull(hovered);
                ExecuteEvents.Execute(hovered,new PointerEventData(events),ExecuteEvents.pointerEnterHandler);
                Assert.AreSame(hovered,events.currentSelectedGameObject);
                Assert.IsFalse(previous.GetComponent<Outline>().enabled);
                Assert.IsTrue(hovered.GetComponent<Outline>().enabled);

                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.A));
                yield return null;
                Assert.AreEqual("Character 3 Button",events.currentSelectedGameObject.name);
                Assert.IsFalse(hovered.GetComponent<Outline>().enabled,
                    "Mouse hover must not leave a second highlight after keyboard navigation.");
                Debug.Log("Keyboard UI: mouse and keyboard share one highlighted selection.");
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);
                Time.timeScale=1;
            }
        }
    }
}
