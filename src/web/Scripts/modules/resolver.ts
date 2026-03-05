export const Modules = {
    indexPage: () => import('./indexPage'),
    newUser: () => import('./NewUser'),
    manager: () => import('./managerIndex'),
    logs: () => import("../manage/logs"),
    newUserInvite:()=>import('./NewUserInvite'),
    login:()=>import('./LogIn'),
    users:()=>import('./users')

};
