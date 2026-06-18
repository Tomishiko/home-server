export const Modules = {
    indexPage: () => import('./indexPage'),
    //newUser: () => import('./NewUser'),
    manager: () => import('./managerIndex'),
    logs: () => import("../manage/logs"),
    login: () => import('./LogIn'),
    users: () => import('./users'),
    indexTable: () => import('./indexTable'),

};
