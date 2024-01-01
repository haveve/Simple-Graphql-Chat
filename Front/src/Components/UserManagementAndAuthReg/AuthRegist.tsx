import React, { useState, useRef } from 'react'
import Auth from './Auth'
import SignUp from './SignUp'
import '../../Styles/LoginLogout.css'
import { setState } from '../../Redux/Slicers/AuthRegSlicer';
import { useTypedDispatch } from '../../Redux/store';

export default function AuthReg() {

    const dispatch = useTypedDispatch();

    const refContainer = useRef<HTMLDivElement>(null)
 const changeToLogin = () => {
        if (refContainer.current) {
            dispatch(setState({state:'idle'}))
            refContainer.current.classList.remove("active")
        }
    }

    const changeToRegister = () => {
        if (refContainer.current) {
            dispatch(setState({state:'idle'}))
            refContainer.current.classList.add("active")
        }
    }

    return <div className='h-100 w-100 d-flex bg-container justify-content-center align-items-center'>
        <div className="container h-75 w-50" id="container" ref={refContainer}>
            <div className="form-container sign-up">
                <SignUp/>
            </div>
            <div className="form-container sign-in">
                <Auth/>
            </div>
            <div className="toggle-container">
                <div className="toggle">
                    <div className="toggle-panel toggle-left">
                        <h1>Welcome Back!</h1>
                        <p>Enter your personal details to use all of site features</p>
                        <button className="hidden" id="login" onClick={changeToLogin}>Sign In</button>
                    </div>
                    <div className="toggle-panel toggle-right">
                        <h1>Hello, Friend!</h1>
                        <p>Register with your personal details to use all of site features</p>
                        <button className="hidden" id="register" onClick={changeToRegister} >Sign Up</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
}