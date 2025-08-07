using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    private Dictionary<KeyCode, Action> keyBindings;
    private Dictionary<int, Action> mouseBindings;
    
    public override void Awake()
    {
        base.Awake();
        Instance.Init();
    }

    protected override void Init()
    {
        // 초기화: 키 바인딩 설정
        keyBindings = new Dictionary<KeyCode, Action>();
        mouseBindings = new Dictionary<int, Action>();  // 마우스 버튼 바인딩 초기화
    }

    void Update()
    {
        CheckInput();
    }

    void CheckInput()
    {
        // 모든 바인딩을 검사하여 입력을 확인
        foreach (var keyBinding in keyBindings)
        {
            if (Input.GetKeyDown(keyBinding.Key))
            {
                keyBinding.Value?.Invoke();  // 키가 눌리면 해당 동작을 실행
            }
        }
        // 마우스 버튼 클릭 체크 
        foreach (var mouseBinding in mouseBindings)
        {
            if (Input.GetMouseButtonDown(mouseBinding.Key))
            {
                mouseBinding.Value?.Invoke();  // 마우스 클릭 이벤트 실행
            }
        }
    }


    public void AddKeyBinding(KeyCode key, Action action)
    {
        if (!keyBindings.ContainsKey(key))
        {
            keyBindings.Add(key, action);
            Debug.Log($"Key {key} added with action.");
        }
        else
        {
            keyBindings[key] += action;
            Debug.LogWarning($"Key {key} is already bound.");
        }
    }
    
    public void AddMouseBinding(int mouseButton, Action action)
    {
        if (!mouseBindings.ContainsKey(mouseButton))
        {
            mouseBindings.Add(mouseButton, action);
            Debug.Log($"Mouse button {mouseButton} added with action.");
        }
        else
        {
            mouseBindings[mouseButton] += action;
            Debug.LogWarning($"Mouse button {mouseButton} is already bound.");
        }
    }
    
    public void RemoveKeyBinding(KeyCode key, Action action)
    {
        if (keyBindings.ContainsKey(key))
        {
            keyBindings[key] -= action;
            Debug.Log($"Action removed from Key {key}.");
        }
        else
        {
            Debug.LogWarning($"Key {key} is not bound.");
        }
    }
    
    public void RemoveMouseBinding(int mouseButton, Action action)
    {
        if (mouseBindings.ContainsKey(mouseButton))
        {
            mouseBindings[mouseButton] -= action;
            Debug.Log($"Mouse button {mouseButton} Remove with action.");
        }
        else
        {
            Debug.LogWarning($"Mouse button {mouseButton} is not bound.");
        }
    }
    
    public void ChangeKeyBinding(KeyCode oldKey, KeyCode newKey)
    {
        if (keyBindings.ContainsKey(oldKey))
        {
            Action action = keyBindings[oldKey];
            keyBindings.Remove(oldKey);
            keyBindings.Add(newKey, action);
            Debug.Log($"Key {oldKey} changed to {newKey}");
        }
        else
        {
            Debug.LogWarning($"Key {oldKey} is not bound.");
        }
    }
    
    public void ChangeMouseBinding(int oldButton, int newButton)
    {
        if (mouseBindings.ContainsKey(oldButton))
        {
            Action action = mouseBindings[oldButton];
            mouseBindings.Remove(oldButton);
            mouseBindings.Add(newButton, action);
            Debug.Log($"Mouse button {oldButton} changed to {newButton}");
        }
        else
        {
            Debug.LogWarning($"Mouse button {oldButton} is not bound.");
        }
    }
}
