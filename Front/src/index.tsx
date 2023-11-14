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
import { getTokenOrNavigate } from './Features/Functions';
import store from './Redux/store';
import { Provider } from 'react-redux'

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

const Router = createBrowserRouter([
  {
    path: '/main',
    element: <Chat />,
    loader: async () => getTokenOrNavigate()
  },
  {
    path: '/',
    element: <StartMenu />,
    loader: async () => getTokenOrNavigate(true),
    children: [
      {
        path: '/',
        element: <Auth />
      },
      {
        path: 'sign-up',
        element: <SignUp />
      },
      {
        path: '/set-password',
        element: <SetPassword />
      }
    ]
  }
])


root.render(
  <Provider store={store}>
    <RouterProvider router={Router} />
  </Provider>
);
