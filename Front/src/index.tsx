import React from 'react';
import ReactDOM from 'react-dom/client';
import './Styles/index.css';
import Chat from './Components/Chat';
import Auth from './Components/Auth';
import StartMenu from './Components/StartMenu';
import SignUp from './Components/SignUp';
import {
  createBrowserRouter,
  RouterProvider
} from "react-router-dom";
import SetPassword from './Components/SetPassword';


const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

const Router = createBrowserRouter([
  {
    path:'/main',
    element:<Chat/>,
  },
  {
    path:'/',
    element:<StartMenu/>,
    children:[
      {
        path:'/',
        element:<Auth/>
      },
      {
        path: 'sign-up',
        element:<SignUp/>
      },
      {
      path:'/set-password',
      element:<SetPassword/>
      }
    ]
  }
])


root.render(
    <RouterProvider router={Router}/>
);
