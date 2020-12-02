using FairyGUI;

public class Window4 : Window
{
    public Window4()
    {
    }

    protected override void OnInit()
    {
        contentPane = UIPackage.CreateObject("ModalWaiting", "TestWin").asCom;
        contentPane.GetChild("n1").onClick.Add(OnClick);
    }

    private void OnClick()
    {
        ShowModalWait();
        Timers.inst.Add(3, 1, (object param) => { CloseModalWait(); });
    }
}