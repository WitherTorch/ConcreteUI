namespace ConcreteUI.Window2
{
    /// <summary>
    /// Virtual Keys
    /// </summary>
    public enum VirtualKey : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Left mouse button
        /// </summary>
        MouseLeftButton = 0x01,
        /// <summary>
        /// Right mouse button
        /// </summary>
        MouseRightButton = 0x02,
        /// <summary>
        /// Control-break processing
        /// </summary>
        Cancel = 0x03,
        /// <summary>
        /// Middle mouse button
        /// </summary>
        MouseMiddleButton = 0x04,    /* NOT contiguous with L & RBUTTON */
        /// <summary>
        /// 	X1 mouse button
        /// </summary>
        MouseXButton1 = 0x05,    /* NOT contiguous with L & RBUTTON */
        /// <summary>
        /// X2 mouse button
        /// </summary>
        MouseXButton2 = 0x06,    /* NOT contiguous with L & RBUTTON */

        // 0x07 - Reserved

        /// <summary>
        /// Backspace key
        /// </summary>
        Back = 0x08,
        /// <summary>
        /// Tab key
        /// </summary>
        Tab = 0x09,

        // 0x0A - 0x0B : Reserved

        /// <summary>
        /// Clear key
        /// </summary>
        Clear = 0x0C,
        /// <summary>
        /// Enter key
        /// </summary>
        Enter = 0x0D,

        // 0x0E - 0x0F : Unassigned

        /// <summary>
        /// Shift key
        /// </summary>
        Shift = 0x10,
        /// <summary>
        /// Ctrl key
        /// </summary>
        Control = 0x11,
        /// <summary>
        /// Alt key
        /// </summary>
        Alt = 0x12,
        /// <summary>
        /// Pause key
        /// </summary>
        Pause = 0x13,
        /// <summary>
        /// Caps lock key
        /// </summary>
        CapsLock = 0x14,

        /// <summary>
        /// IME Kana mode
        /// </summary>
        ImeKana = 0x15,
        /// <summary>
        /// IME Hangul mode
        /// </summary>
        ImeHangul = 0x15,
        /// <summary>
        /// IME On
        /// </summary>
        ImeOn = 0x16,
        /// <summary>
        /// IME Junja mode
        /// </summary>
        ImeJunja = 0x17,
        /// <summary>
        /// IME final mode
        /// </summary>
        ImeFinal = 0x18,
        /// <summary>
        /// IME Hanja mode
        /// </summary>
        ImeHanja = 0x19,
        /// <summary>
        /// IME Kanji mode
        /// </summary>
        ImeKanji = 0x19,
        /// <summary>
        /// IME Off
        /// </summary>
        ImeOff = 0x1A,

        /// <summary>
        /// Esc key
        /// </summary>
        Escape = 0x1B,

        /// <summary>
        /// IME convert
        /// </summary>
        ImeConvert = 0x1C,
        /// <summary>
        /// IME nonconvert
        /// </summary>
        ImeNonConvert = 0x1D,
        /// <summary>
        /// IME accept
        /// </summary>
        ImeAccept = 0x1E,
        /// <summary>
        /// IME mode change request
        /// </summary>
        ImeModeChange = 0x1F,

        /// <summary>
        /// Whitespace key
        /// </summary>
        Whitespace = 0x20,
        /// <summary>
        /// Page up key
        /// </summary>
        PageUp = 0x21,
        /// <summary>
        /// Page down key
        /// </summary>
        PageDown = 0x22,
        /// <summary>
        /// End key
        /// </summary>
        End = 0x23,
        /// <summary>
        /// Home key
        /// </summary>
        Home = 0x24,
        /// <summary>
        /// Left arrow key
        /// </summary>
        LeftArrow = 0x25,
        /// <summary>
        /// Up arrow key
        /// </summary>
        UpArrow = 0x26,
        /// <summary>
        /// Right arrow key
        /// </summary>
        RightArrow = 0x27,
        /// <summary>
        /// Down arrow key
        /// </summary>
        DownArrow = 0x28,
        /// <summary>
        /// Select key
        /// </summary>
        Select = 0x29,
        /// <summary>
        /// Print key
        /// </summary>
        Print = 0x2A,
        /// <summary>
        /// Execute key
        /// </summary>
        Execute = 0x2B,
        /// <summary>
        /// Print screen key
        /// </summary>
        PrintScreen = 0x2C,
        /// <summary>
        /// Insert key
        /// </summary>
        Insert = 0x2D,
        /// <summary>
        /// Delete key
        /// </summary>
        Delete = 0x2E,
        /// <summary>
        /// Help key
        /// </summary>
        Help = 0x2F,

        /// <summary>
        /// 0 key
        /// </summary>
        NumberKey_0 = 0x30,
        /// <summary>
        /// 1 key
        /// </summary>
        NumberKey_1 = 0x31,
        /// <summary>
        /// 2 key
        /// </summary>
        NumberKey_2 = 0x32,
        /// <summary>
        /// 3 key
        /// </summary>
        NumberKey_3 = 0x33,
        /// <summary>
        /// 4 key
        /// </summary>
        NumberKey_4 = 0x34,
        /// <summary>
        /// 5 key
        /// </summary>
        NumberKey_5 = 0x35,
        /// <summary>
        /// 6 key
        /// </summary>
        NumberKey_6 = 0x36,
        /// <summary>
        /// 7 key
        /// </summary>
        NumberKey_7 = 0x37,
        /// <summary>
        /// 8 key
        /// </summary>
        NumberKey_8 = 0x38,
        /// <summary>
        /// 9 key
        /// </summary>
        NumberKey_9 = 0x39,

        // 0x3A - 0x40 : Unassigned

        /// <summary>
        /// A key
        /// </summary>
        A = 0x41,
        /// <summary>
        /// B key
        /// </summary>
        B = 0x42,
        /// <summary>
        /// C key
        /// </summary>
        C = 0x43,
        /// <summary>
        /// D key
        /// </summary>
        D = 0x44,
        /// <summary>
        /// E key
        /// </summary>
        E = 0x45,
        /// <summary>
        /// F key
        /// </summary>
        F = 0x46,
        /// <summary>
        /// G key
        /// </summary>
        G = 0x47,
        /// <summary>
        /// H key
        /// </summary>
        H = 0x48,
        /// <summary>
        /// I key
        /// </summary>
        I = 0x49,
        /// <summary>
        /// J key
        /// </summary>
        J = 0x4A,
        /// <summary>
        /// K key
        /// </summary>
        K = 0x4B,
        /// <summary>
        /// L key
        /// </summary>
        L = 0x4C,
        /// <summary>
        /// M key
        /// </summary>
        M = 0x4D,
        /// <summary>
        /// N key
        /// </summary>
        N = 0x4E,
        /// <summary>
        /// O key
        /// </summary>
        O = 0x4F,
        /// <summary>
        /// P key
        /// </summary>
        P = 0x50,
        /// <summary>
        /// Q key
        /// </summary>
        Q = 0x51,
        /// <summary>
        /// R key
        /// </summary>
        R = 0x52,
        /// <summary>
        /// S key
        /// </summary>
        S = 0x53,
        /// <summary>
        /// T key
        /// </summary>
        T = 0x54,
        /// <summary>
        /// U key
        /// </summary>
        U = 0x55,
        /// <summary>
        /// V key
        /// </summary>
        V = 0x56,
        /// <summary>
        /// W key
        /// </summary>
        W = 0x57,
        /// <summary>
        /// X key
        /// </summary>
        X = 0x58,
        /// <summary>
        /// Y key
        /// </summary>
        Y = 0x59,
        /// <summary>
        /// Z key
        /// </summary>
        Z = 0x5A,

        /// <summary>
        /// Left Windows logo key
        /// </summary>
        LeftWindows = 0x5B,
        /// <summary>
        /// Right Windows logo key
        /// </summary>
        RightWindows = 0x5C,
        /// <summary>
        /// Application key
        /// </summary>
        Applications = 0x5D,

        // 0x5E : Reverved

        /// <summary>
        /// Computer Sleep key
        /// </summary>
        Sleep = 0x5F,

        /// <summary>
        /// Numeric keypad 0 key
        /// </summary>
        Numpad0 = 0x60,
        /// <summary>
        /// Numeric keypad 1 key
        /// </summary>
        Numpad1 = 0x61,
        /// <summary>
        /// Numeric keypad 2 key
        /// </summary>
        Numpad2 = 0x62,
        /// <summary>
        /// Numeric keypad 3 key
        /// </summary>
        Numpad3 = 0x63,
        /// <summary>
        /// Numeric keypad 4 key
        /// </summary>
        Numpad4 = 0x64,
        /// <summary>
        /// Numeric keypad 5 key
        /// </summary>
        Numpad5 = 0x65,
        /// <summary>
        /// Numeric keypad 6 key
        /// </summary>
        Numpad6 = 0x66,
        /// <summary>
        /// Numeric keypad 7 key
        /// </summary>
        Numpad7 = 0x67,
        /// <summary>
        /// Numeric keypad 8 key
        /// </summary>
        Numpad8 = 0x68,
        /// <summary>
        /// Numeric keypad 9 key
        /// </summary>
        Numpad9 = 0x69,
        /// <summary>
        /// Multiply key
        /// </summary>
        Multiply = 0x6A,
        /// <summary>
        /// Add key
        /// </summary>
        Add = 0x6B,
        /// <summary>
        /// Separator key
        /// </summary>
        Separator = 0x6C,
        /// <summary>
        /// Subtract key
        /// </summary>
        Subtract = 0x6D,
        /// <summary>
        /// Decimal key
        /// </summary>
        Decimal = 0x6E,
        /// <summary>
        /// Divide key
        /// </summary>
        Divide = 0x6F,
        /// <summary>
        /// F1 key
        /// </summary>
        F1 = 0x70,
        /// <summary>
        /// F2 key
        /// </summary>
        F2 = 0x71,
        /// <summary>
        /// F3 key
        /// </summary>
        F3 = 0x72,
        /// <summary>
        /// F4 key
        /// </summary>
        F4 = 0x73,
        /// <summary>
        /// F5 key
        /// </summary>
        F5 = 0x74,
        /// <summary>
        /// F6 key
        /// </summary>
        F6 = 0x75,
        /// <summary>
        /// F7 key
        /// </summary>
        F7 = 0x76,
        /// <summary>
        /// F8 key
        /// </summary>
        F8 = 0x77,
        /// <summary>
        /// F9 key
        /// </summary>
        F9 = 0x78,
        /// <summary>
        /// F10 key
        /// </summary>
        F10 = 0x79,
        /// <summary>
        /// F11 key
        /// </summary>
        F11 = 0x7A,
        /// <summary>
        /// F12 key
        /// </summary>
        F12 = 0x7B,
        /// <summary>
        /// F13 key
        /// </summary>
        F13 = 0x7C,
        /// <summary>
        /// F14 key
        /// </summary>
        F14 = 0x7D,
        /// <summary>
        /// F15 key
        /// </summary>
        F15 = 0x7E,
        /// <summary>
        /// F16 key
        /// </summary>
        F16 = 0x7F,
        /// <summary>
        /// F17 key
        /// </summary>
        F17 = 0x80,
        /// <summary>
        /// F18 key
        /// </summary>
        F18 = 0x81,
        /// <summary>
        /// F19 key
        /// </summary>
        F19 = 0x82,
        /// <summary>
        /// F20 key
        /// </summary>
        F20 = 0x83,
        /// <summary>
        /// F21 key
        /// </summary>
        F21 = 0x84,
        /// <summary>
        /// F22 key
        /// </summary>
        F22 = 0x85,
        /// <summary>
        /// F23 key
        /// </summary>
        F23 = 0x86,
        /// <summary>
        /// F24 Key
        /// </summary>
        F24 = 0x87,

        // 0x88 - 0x8F : UI navigation
        /// <summary>
        /// Reserved. Be used to UI navigation
        /// </summary>
        NavigationView = 0x88, // reserved
        /// <summary>
        /// Reserved. Be used to UI navigation
        /// </summary>
        NavigationMenu = 0x89, // reserved
        /// <summary>
        /// Reserved. Be used to UI navigation
        /// </summary>
        NavigationUp = 0x8A, // reserved
        /// <summary>
        /// Reserved. Be used to UI navigation
        /// </summary>
        NavigationDown = 0x8B, // reserved
        /// <summary>
        /// Reserved. Be used to UI navigation
        /// </summary>
        NavigationLeft = 0x8C, // reserved
        /// <summary>
        /// Reserved. Be used to UI navigation
        /// </summary>
        NavigationRight = 0x8D, // reserved
        /// <summary>
        /// Reserved. Be used to UI navigation
        /// </summary>
        NavigationAccept = 0x8E, // reserved
        /// <summary>
        /// Reserved. Be used to UI navigation
        /// </summary>
        NavigationCancel = 0x8F, // reserved

        /// <summary>
        /// Num lock key
        /// </summary>
        NumLock = 0x90,
        /// <summary>
        /// Scroll lock key
        /// </summary>
        ScrollLock = 0x91,

        // NEC PC-9800 keyboard definitions
        /// <summary>
        /// (OEM specific, NEC PC-9800 keyboard)
        /// '=' key on numpad
        /// </summary>
        OemNecEqual = 0x92,

        // Fujitsu/OASYS keyboard definitions
        /// <summary>
        /// (OEM specific, Fujitsu/OASYS keyboard)
        /// 'Dictionary' key
        /// </summary>
        OemFujitsuJisho = 0x92,
        /// <summary>
        /// (OEM specific, Fujitsu/OASYS keyboard)
        /// 'Unregister word' key
        /// </summary>
        OemFujitsuMasshou = 0x93,
        /// <summary>
        /// (OEM specific, Fujitsu/OASYS keyboard)
        /// 'Register word' key
        /// </summary>
        OemFujitsuTouroku = 0x94,
        /// <summary>
        /// (OEM specific, Fujitsu/OASYS keyboard)
        /// 'Left OYAYUBI' key
        /// </summary>
        OemFujitsuLoya = 0x95,
        /// <summary>
        /// (OEM specific, Fujitsu/OASYS keyboard)
        /// 'Right OYAYUBI' key
        /// </summary>
        OemFujitsuRoya = 0x96,

        // 0x97 - 0x9F : Unassigned

        /// <summary>
        /// Left Shift key
        /// </summary>
        /// <remarks>
        /// Used only as parameters to GetAsyncKeyState() and GetKeyState() in User32.dll.<br/>
        /// No other API or message will distinguish left and right keys in this way.
        /// </remarks>
        LeftShift = 0xA0,
        /// <summary>
        /// Right Shift key
        /// </summary>
        /// <remarks>
        /// Used only as parameters to GetAsyncKeyState() and GetKeyState() in User32.dll.<br/>
        /// No other API or message will distinguish left and right keys in this way.
        /// </remarks>
        RightShift = 0xA1,
        /// <summary>
        /// Left Ctrl key
        /// </summary>
        /// <remarks>
        /// Used only as parameters to GetAsyncKeyState() and GetKeyState() in User32.dll.<br/>
        /// No other API or message will distinguish left and right keys in this way.
        /// </remarks>
        LeftControl = 0xA2,
        /// <summary>
        /// Right Ctrl key
        /// </summary>
        /// <remarks>
        /// Used only as parameters to GetAsyncKeyState() and GetKeyState() in User32.dll.<br/>
        /// No other API or message will distinguish left and right keys in this way.
        /// </remarks>
        RightControl = 0xA3,
        /// <summary>
        /// Left Alt key
        /// </summary>
        /// <remarks>
        /// Used only as parameters to GetAsyncKeyState() and GetKeyState() in User32.dll.<br/>
        /// No other API or message will distinguish left and right keys in this way.
        /// </remarks>
        LeftAlt = 0xA4,
        /// <summary>
        /// Right Alt key
        /// </summary>
        /// <remarks>
        /// Used only as parameters to GetAsyncKeyState() and GetKeyState() in User32.dll.<br/>
        /// No other API or message will distinguish left and right keys in this way.
        /// </remarks>
        RightAlt = 0xA5,

        /// <summary>
        /// Browser Back key
        /// </summary>
        BrowserBack = 0xA6,
        /// <summary>
        /// Browser Forward key
        /// </summary>
        BrowserForward = 0xA7,
        /// <summary>
        /// Browser Refresh key
        /// </summary>
        BrowserRefresh = 0xA8,
        /// <summary>
        /// Browser Stop key
        /// </summary>
        BrowserStop = 0xA9,
        /// <summary>
        /// Browser Search key
        /// </summary>
        BrowserSearch = 0xAA,
        /// <summary>
        /// Browser Favorites key
        /// </summary>
        BrowserFavorites = 0xAB,
        /// <summary>
        /// Browser Start and Home key
        /// </summary>
        BrowserHome = 0xAC,

        /// <summary>
        /// Volume Mute key
        /// </summary>
        VolumeMute = 0xAD,
        /// <summary>
        /// Volume Down key
        /// </summary>
        VolumeDown = 0xAE,
        /// <summary>
        /// Volume Up key
        /// </summary>
        VolumeUp = 0xAF,

        /// <summary>
        /// Next Track key
        /// </summary>
        MediaNextTrack = 0xB0,
        /// <summary>
        /// Previous Track key
        /// </summary>
        MediaPreviousTrack = 0xB1,
        /// <summary>
        /// Stop Media key
        /// </summary>
        MediaStop = 0xB2,
        /// <summary>
        /// Play/Pause Media key
        /// </summary>
        MediaPlayPause = 0xB3,

        /// <summary>
        /// Start Mail key
        /// </summary>
        LaunchMail = 0xB4,
        /// <summary>
        /// Select Media key
        /// </summary>
        LaunchMediaSelect = 0xB5,
        /// <summary>
        /// Start Application 1 key
        /// </summary>
        LaunchApplication1 = 0xB6,
        /// <summary>
        /// Start Application 2 key
        /// </summary>
        LaunchApplication2 = 0xB7,

        // 0xB8 - 0xB9 : reserved

        /// <summary>
        /// (OEM specific) For the US ANSI keyboard , the Semiсolon and Colon key
        /// </summary>
        Oem1 = 0xBA,
        /// <summary>
        /// (OEM specific) For any country/region, the Equals and Plus key
        /// </summary>
        OemPlus = 0xBB,
        /// <summary>
        /// (OEM specific) For any country/region, the Comma and Less Than key
        /// </summary>
        OemComma = 0xBC,
        /// <summary>
        /// (OEM specific) For any country/region, the Dash and Underscore key
        /// </summary>
        OemMinus = 0xBD,
        /// <summary>
        /// (OEM specific) For any country/region, the Period and Greater Than key
        /// </summary>
        OemPeriod = 0xBE,
        /// <summary>
        /// (OEM specific) For the US ANSI keyboard, the Forward Slash and Question Mark key
        /// </summary>
        Oem2 = 0xBF,
        /// <summary>
        /// (OEM specific) For the US ANSI keyboard, the Grave Accent and Tilde key
        /// </summary>
        Oem3 = 0xC0,

        // 0xC1 - 0xC2 : reserved

        // Gamepad input
        /// <summary>
        /// (Reserved) Gamepad A key
        /// </summary>
        GamepadA = 0xC3,
        /// <summary>
        /// (Reserved) Gamepad B key
        /// </summary>
        GamepadB = 0xC4,
        /// <summary>
        /// (Reserved) Gamepad X key
        /// </summary>
        GamepadX = 0xC5,
        /// <summary>
        /// (Reserved) Gamepad Y key
        /// </summary>
        GamepadY = 0xC6,
        /// <summary>
        /// (Reserved) Gamepad right shoulder key
        /// </summary>
        GamepadRightShoulder = 0xC7,
        /// <summary>
        /// (Reserved) Gamepad left shoulder key
        /// </summary>
        GamepadLeftShoulder = 0xC8,
        /// <summary>
        /// (Reserved) Gamepad left trigger key
        /// </summary>
        GamepadLeftTrigger = 0xC9,
        /// <summary>
        /// (Reserved) Gamepad right trigger key
        /// </summary>
        GamepadRightTrigger = 0xCA,
        /// <summary>
        /// (Reserved) Gamepad D-Pad up key
        /// </summary>
        GamepadDPadUp = 0xCB,
        /// <summary>
        /// (Reserved) Gamepad D-Pad down key
        /// </summary>
        GamepadDPadDown = 0xCC,
        /// <summary>
        /// (Reserved) Gamepad D-Pad left key
        /// </summary>
        GamepadDPadLeft = 0xCD,
        /// <summary>
        /// (Reserved) Gamepad D-Pad right key
        /// </summary>
        GamepadDPadRight = 0xCE,
        /// <summary>
        /// (Reserved) Gamepad menu key
        /// </summary>
        GamepadMenu = 0xCF,
        /// <summary>
        /// (Reserved) Gamepad view key
        /// </summary>
        GamepadView = 0xD0,
        /// <summary>
        /// (Reserved) Gamepad left thumbstick button
        /// </summary>
        GamepadLeftThumbstickButton = 0xD1,
        /// <summary>
        /// (Reserved) Gamepad right thumbstick button
        /// </summary>
        GamepadRightThumbstickButton = 0xD2,
        /// <summary>
        /// (Reserved) Gamepad left thumbstick up key
        /// </summary>
        GamepadLeftThumbstickUp = 0xD3,
        /// <summary>
        /// (Reserved) Gamepad left thumbstick down key
        /// </summary>
        GamepadLeftThumbstickDown = 0xD4,
        /// <summary>
        /// (Reserved) Gamepad left thumbstick right key
        /// </summary>
        GamepadLeftThumbstickRight = 0xD5,
        /// <summary>
        /// (Reserved) Gamepad left thumbstick left key
        /// </summary>
        GamepadLeftThumbstickLeft = 0xD6,
        /// <summary>
        /// (Reserved) Gamepad right thumbstick up key
        /// </summary>
        GamepadRightThumbstickUp = 0xD7,
        /// <summary>
        /// (Reserved) Gamepad right thumbstick down key
        /// </summary>
        GamepadRightThumbstickDown = 0xD8,
        /// <summary>
        /// (Reserved) Gamepad right thumbstick right key
        /// </summary>
        GamepadRightThumbstickRight = 0xD9,
        /// <summary>
        /// (Reserved) Gamepad right thumbstick left key
        /// </summary>
        GamepadRightThumbstickLeft = 0xDA,

        /// <summary>
        /// (OEM specific) For the US ANSI keyboard, the Left Brace key
        /// </summary>
        Oem4 = 0xDB,
        /// <summary>
        /// (OEM specific) For the US ANSI keyboard, the Backslash and Pipe key
        /// </summary>
        Oem5 = 0xDC,
        /// <summary>
        /// (OEM specific) For the US ANSI keyboard, the Right Brace key
        /// </summary>
        Oem6 = 0xDD,
        /// <summary>
        /// (OEM specific) For the US ANSI keyboard, the Apostrophe and Double Quotation Mark key
        /// </summary>
        Oem7 = 0xDE,
        /// <summary>
        /// (OEM specific) For the Canadian CSA keyboard, the Right Ctrl key
        /// </summary>
        Oem8 = 0xDF,

        // 0xE0 : Reserved

        // Various extended or enhanced keyboards
        /// <summary>
        /// (OEM specific, Japanese AX keyboard) 'AX' Key
        /// </summary>
        OemAx = 0xE1,
        /// <summary>
        /// (OEM specific) For the European ISO keyboard, the Backslash and Pipe key
        /// </summary>
        Oem102 = 0xE2,
        /// <summary>
        /// (OEM specific, ICO only) 'Help' key
        /// </summary>
        OemIcoHelp = 0xE3,
        /// <summary>
        /// (OEM specific, ICO only) '00' key
        /// </summary>
        OemIco00 = 0xE4,

        /// <summary>
        /// IME PROCESS key
        /// </summary>
        ImeProcess = 0xE5,

        /// <summary>
        /// (OEM specific, ICO only) 'Clear' key
        /// </summary>
        OemIcoClear = 0xE6,

        /// <summary>
        /// Used to pass Unicode characters as if they were keystrokes.<br/>
        /// This key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods.
        /// </summary>
        Packet = 0xE7,

        // 0xE8 : Unassigned

        /*
         * Nokia/Ericsson definitions
         */
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'Reset' key
        /// </summary>
        OemReset = 0xE9,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'Jump' key
        /// </summary>
        OemJump = 0xEA,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'PA1' key
        /// </summary>
        OemPA1 = 0xEB,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'PA2' key
        /// </summary>
        OemPA2 = 0xEC,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'PA3' key
        /// </summary>
        OemPA3 = 0xED,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'WsCtrl' key
        /// </summary>
        OemWsCtrl = 0xEE,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'CuSel' key
        /// </summary>
        OemCuSel = 0xEF,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'Attn' key
        /// </summary>
        OemAttn = 0xF0,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'Finish' key
        /// </summary>
        OemFinish = 0xF1,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'Copy' key
        /// </summary>
        OemCopy = 0xF2,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'Auto' key
        /// </summary>
        OemAuto = 0xF3,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'Enlw' key
        /// </summary>
        OemEnlw = 0xF4,
        /// <summary>
        /// (OEM specific, Nokia keyboard) 'BackTab' key
        /// </summary>
        OemBackTab = 0xF5,

        /// <summary>
        /// Attn key
        /// </summary>
        Attn = 0xF6,
        /// <summary>
        /// CrSel key
        /// </summary>
        CrSel = 0xF7,
        /// <summary>
        /// ExSel key
        /// </summary>
        ExSel = 0xF8,
        /// <summary>
        /// Erase EOF key
        /// </summary>
        EraseEof = 0xF9,
        /// <summary>
        /// Play key
        /// </summary>
        Play = 0xFA,
        /// <summary>
        /// Zoom key
        /// </summary>
        Zoom = 0xFB,
        /// <summary>
        /// Reserved
        /// </summary>
        NoName = 0xFC,
        /// <summary>
        /// PA1 key
        /// </summary>
        PA1 = 0xFD,
        /// <summary>
        /// (Oem specific) Clear key
        /// </summary>
        OemClear = 0xFE,
    }
}
