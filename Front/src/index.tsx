import React, { StrictMode } from 'react';
import ReactDOM from 'react-dom/client';
import './Styles/index.css';
import Chat from './Components/Chat';
import {
  createBrowserRouter,
  RouterProvider
} from "react-router-dom";
import SetPassword from './Components/AuthRegReset/SetPassword';
import { getTokenOrNavigate } from './Features/Functions';
import store from './Redux/store';
import { Provider } from 'react-redux'
import NotFound from './Components/NotFoundPage';
import AuthReg from './Components/AuthRegReset/AuthRegist';
import ResetPassword from './Components/AuthRegReset/ResetPassword';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

const Router = createBrowserRouter([
  {
    path:"*",
    element:<NotFound/>
  },
  {
    path: '/main',
    element: <Chat />,
    loader: async () => getTokenOrNavigate()
  },
  {
    path: '/',
    element: <AuthReg />,
    loader: async () => getTokenOrNavigate(true)
  },
  {
    path: '/set-password',
    element: <SetPassword/>
  },
  {
    path: '/reset-password',
    element:<ResetPassword/>
  }
])


root.render(
  <StrictMode>
    <Provider store={store}>
      <RouterProvider router={Router} />
    </Provider>
  </StrictMode>
);
