import React, { useState, useEffect } from 'react';

import WikiSidebarComponent from './WikiSidebarComponent'

export default function WikiComponent() {
    const [QnAs, setQnAs] = useState([]);

    useEffect(() => {
        fetch('https://extractqnaapi2.azure-api.net/ConversationSummary',{
                method: 'GET',
                accept: 'text/plain'
            }).then((response) => response.json())
            .then((data) => {
                console.log("data is " + JSON.stringify(data));
                setQnAs(data);
            })
            .catch((err) => {
                console.log("Failed with error" + err);
            });
    }, []);

    return (
        <div className="wiki-section">
            <WikiSidebarComponent />
            <div className="wiki-main-content">
                <h1>Channel Wiki</h1>
                <div className="wiki-content">
                   {QnAs.map((QnA) => {
                    return (
                        <div className="wiki-content-item" key={QnA.conversationId}>
                            <h2 className="wiki-item-title">{QnA.question}</h2>
                            <p className="post-body">{QnA.summary}</p>
                        </div>
                    );
                  })}
               </div>
            </div>
        </div>
    )
}