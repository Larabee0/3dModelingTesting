using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBase : MonoBehaviour
{
    public bool OneKeyIsPressed = false;
    public void OneKeyDown(InputAction.CallbackContext _context)
    {
        if (_context.started)
        {
            OneKeyIsPressed = true;
        }
        if (_context.canceled)
        {
            OneKeyIsPressed = false;
        }
    }

}
