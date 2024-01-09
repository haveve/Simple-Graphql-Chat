import React, { useState, forwardRef, useEffect } from 'react'
import { Container, Row, Col, Form, Button, Dropdown, DropdownButton } from 'react-bootstrap';
import { useTranslation } from 'react-i18next';
import { SetLanguage, FullLangugeNames, Languages } from '../../LanguageConfig';
export default function SelectLanguage() {

    const { t } = useTranslation();

    return <DropdownButton variant='info' title={t("SelectLanguage")} drop={"end"} className='additional-info' >
        {Object.values(Languages)
            .map(value => <Dropdown.Item onClick={() => SetLanguage(value as Languages)} className='additional-info-node'>{`${FullLangugeNames[value]} (${value})`}</Dropdown.Item>)}
    </DropdownButton>
}
