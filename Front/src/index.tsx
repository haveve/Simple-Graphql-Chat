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
        path: 'sign-in',
        element:<SignUp/>
      }
    ]
  }
])


root.render(
    <RouterProvider router={Router}/>
);
