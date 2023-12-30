import { Button, Card, Form, FormText } from "react-bootstrap";
import React, { FormEvent, useState } from "react";
import { ajaxVerifyUserCode } from "../../Requests/AuthorizationRequests";
import NotificationModalWindow,{ MessageType } from "../../Components/Service/NotificationModalWindow";
import { useNavigate } from "react-router-dom";

export default function Auth2f(){
    const queryParameters = new URLSearchParams(window.location.search)
    const token = queryParameters.get("tempToken")
    const redirect = useNavigate()

    const [error,setError] = useState('')

    const [code,setCode] = useState('')

    return <div className="d-flex justify-content-center">
        {token?
        <Card className="m-5 p-4 w-25 rounded">
            <Form.Label className="h5">Enter one-time code</Form.Label>
            <Form.Control
            onChange={(event)=>setCode(event.target.value)}></Form.Control>
            <Button variant="outline-warning" className="mt-4" onClick={()=>{
                ajaxVerifyUserCode(token,code,"/").subscribe({
                    error:()=>setError("Codes are not matched"),
                    next:(response)=>redirect(response)
                })
            }}> Verify 2f code</Button>
        </Card>:null}
   </div>

}
