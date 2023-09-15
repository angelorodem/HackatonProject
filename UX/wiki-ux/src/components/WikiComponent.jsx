import React, { useState, useEffect } from 'react';

 

export default function WikiComponent() {

    const [QnAs, setQnAs] = useState([]);
    const [channelName, setChannelName] = useState("dri_channel");
    const [title, setTitle] = useState("DRI Channel");

 

    useEffect(() => {

        fetch(`https://localhost:7035/ConversationSummary/GetTeamsChannelSummary?channelName=all`, {

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

    }, [channelName]);

   

    return (

        <div className="wiki-section">

            <div className="wiki-sidebar">

                <h2>Channels</h2>

                <ul>

                    <li>

                        <button
                            onClick={() => {setTitle("DRI Channel"); setChannelName("dri_channel")}}
                        >DRI Channel</button>

                    </li>

                    <li>

                        <button
                            onClick={() => {setTitle("Engineering Channel"); setChannelName("engineering_channel")}}

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