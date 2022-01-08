using System;
using BlazorBoilerplate.Shared.Dto.Session;

namespace BlazorBoilerplate.Shared.Services
{
    public class SessionState{
        public bool selected;
        public string id;
        public string selectedAutoML;
        public GetSessionResponseDto session;

        public SessionState()
        {
            selected = false;
        }
        public void ChangeState(){
            if(selected){
                selected = false;
            }else{
                selected = true;
            }
            NotifyDataChanged();
        }
        public event Action OnChange;
        private void NotifyDataChanged() => OnChange?.Invoke();
    }
}