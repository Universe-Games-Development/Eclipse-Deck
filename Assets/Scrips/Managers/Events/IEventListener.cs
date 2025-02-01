using System.Collections.Generic;

public interface IEventListener {
    public object OnEventReceived(object data);
}
