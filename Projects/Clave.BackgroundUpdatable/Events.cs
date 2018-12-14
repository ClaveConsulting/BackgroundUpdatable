using System;

namespace Clave.BackgroundUpdatable
{
    public delegate void BackgroundUpdateStartedHandler();

    public delegate void BackgroundUpdateFailedHandler(Exception exception);

    public delegate void BackgroundUpdateSucceededHandler(object value);
}