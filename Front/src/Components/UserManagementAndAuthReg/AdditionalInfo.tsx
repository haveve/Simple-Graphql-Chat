import React, { useState, forwardRef, useEffect, DOMElement, ReactElement } from 'react'
import { Container, Row, Col, Form, Button, Dropdown } from 'react-bootstrap';
import { _2fAuthResult, ajaxForLogout, GetRefresh, TokenErrorHandler } from '../../Requests/AuthorizationRequests';
import { useTypedSelector } from '../../Redux/store';
import Drop2factorAuht from './Drop2f';
import Set2factorAuth from './Set2f';
import { ajaxFor2fAuth, ajaxFor2fDrop } from '../../Requests/AuthorizationRequests';
import { setState, setError } from '../../Redux/Slicers/UserSlicer';
import { useTypedDispatch } from '../../Redux/store';
import UserSettings from './UserSettings';
import { useTranslation } from 'react-i18next';

export default function AdditionalInfo() {

    const { t } = useTranslation();

    const key2Auth = useTypedSelector(store => store.user.user?.key2Auth);
    const _2fText = key2Auth ? t("Drop2f") : t("Set2f");

    const dispatch = useTypedDispatch()

    const [isVisibleDrop, setVisibleDrop] = useState(false)
    const [isVisibleSet, setVisibleSet] = useState(false)
    const [isVisibleSetting, setVisibleSetting] = useState(false)

    const [_2fAuthCode, set2fAuthCode] = useState<null | _2fAuthResult>(null)

    const logOutHandler = () => {
        ajaxForLogout(GetRefresh()).subscribe()
        TokenErrorHandler()
    }

    const set2fHandler = () => {
        dispatch(setState("pending"))
        ajaxFor2fAuth().subscribe({
            next: (data) => {
                set2fAuthCode(data)
                setVisibleSet(true)
                dispatch(setState("idle"))
            },
            error: () => {
                dispatch(setError(t('DefaultErrorMessage')))
            }
        })
    }

    const drop2fHandler = () => {
        setVisibleDrop(true)
    }

    const userSettingsHandler = () => {
        setVisibleSetting(true);
    }

    const _2fHandler = key2Auth ? drop2fHandler : set2fHandler

    const mainMenuToggle = <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" fill="gray" className="custom-svg bi bi-three-dots-vertical" viewBox="0 0 16 16">
        <path d="M9.5 13a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0m0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0m0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0" />
    </svg>

    return <Dropdown className='additional-info'>
        <Dropdown.Toggle as={CustomToggleFactory(mainMenuToggle)} id="dropdown-basic">
        </Dropdown.Toggle>
        <Dropdown.Menu>
            <Dropdown.Item className='additional-info-node' onClick={userSettingsHandler}>{t('Settings')}</Dropdown.Item>
            <Dropdown.Item className='additional-info-node' onClick={_2fHandler}>{_2fText}</Dropdown.Item>
            <Dropdown.Item className='additional-info-node' onClick={logOutHandler}>{t('LogOut')}</Dropdown.Item>
        </Dropdown.Menu>
        <Drop2factorAuht isVisibleDrop2fa={isVisibleDrop} setVisibleDrop2fa={setVisibleDrop} />
        <Set2factorAuth _2fAuthData={_2fAuthCode} isVisibleSet2fa={isVisibleSet} setVisibleSet2fa={setVisibleSet} />
        <UserSettings setVisible={setVisibleSetting} isVisible={isVisibleSetting} />
    </Dropdown>
}

const CustomToggleFactory = (child: ReactElement) => React.forwardRef<any, { children: any, onClick: any }>(({ children, onClick }, ref) => (
    <a
        href=""
        ref={ref}
        onClick={(e) => {
            e.preventDefault();
            onClick(e);
        }}
    >
        {child}
    </a>
));
