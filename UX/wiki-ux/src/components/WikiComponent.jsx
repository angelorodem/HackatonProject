import React, { useState, useEffect } from 'react';

export default function WikiComponent() {
    const [QnAs, setQnAs] = useState([]);
    const [title, setTitle] = useState("DRI Channel");

    useEffect(() => {
        fetch(`https://extractqnaapi2.azure-api.net/ConversationSummary?channelName=${title}`, {
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
    }, [title]);

    // async function fetchData(channelName) {
    //     try {
    //         fetch(`https://extractqnaapi2.azure-api.net/ConversationSummary?channelName=${channelName}`, {
    //         method: 'GET',
    //         accept: 'text/plain'
    //     }).then((response) => response.json())
    //         .then((data) => {
    //             console.log("data is " + JSON.stringify(data));
    //             setQnAs(data);
    //         })
    //         .catch((err) => {
    //             console.log("Failed with error" + err);
    //         });
    //     } catch (error) {
    //         console.error("Error fetching data: ", error);
    //     }
    // }
    return (
        <div className="wiki-section">
            <div className="wiki-sidebar">
                <h2>Channels</h2>
                <ul>
                    <li>
                        <button

                            onClick={() => setTitle("DRI Channel")}
                        >DRI Channel</button>
                    </li>
                    <li>
                        <button 
                            href = ""
                            onClick={() => setTitle("Engineering Channel")}
                         >Engineering Channel
                        </button>
                    </li>
                </ul>
            </div>
            <div className="wiki-main-content">
                <h1>{title} Wiki</h1>
                <div className="wiki-content">
                    {QnAs && QnAs.length !== 0 &&  QnAs.map&&  QnAs.map((QnA) => {
                        return (
                            <div key={QnA.conversationId} className="wiki-content-item">
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